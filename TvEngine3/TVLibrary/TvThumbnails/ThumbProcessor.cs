
#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TvDatabase;
using TvEngine.Events;
using TvLibrary.Interfaces;
using TvLibrary.Log;

namespace TvThumbnails
{
  public class ThumbProcessor
  {
    private readonly ProcessingQueue _queue;

    public ThumbProcessor()
    {
      _queue = new ProcessingQueue(DoWork);
    }

    ~ThumbProcessor()
    {
      _queue.Dispose();
    }

    public void Start()
    {
      if (Thumbs.Enabled)
      {
        Thumbs.LoadSettings();
        Thumbs.CreateFolders();

        _queue.EnqueueTask(Recording.ListAll().Select(recording => recording.FileName).ToList());

        GlobalServiceProvider.Instance.Get<ITvServerEvent>().OnTvServerEvent += OnTvServerEvent;
      }
      else
      {
        Stop();
      }
    }

    public void Stop()
    {
      if (GlobalServiceProvider.Instance.IsRegistered<ITvServerEvent>())
      {
        GlobalServiceProvider.Instance.Get<ITvServerEvent>().OnTvServerEvent -= OnTvServerEvent;
      }
    }

    public string GetThumbnailFolder()
    {
      return Thumbs.ThumbnailFolder;
    }

    // TODO Clean up old thumbs ???

    private void DoWork(string recFileName)
    {
      Log.Info("ThumbProcessor - creating thumb for {0}", recFileName);

      try
      {
        string thumbNail = string.Format("{0}\\{1}{2}", Thumbs.ThumbnailFolder,
          Path.ChangeExtension(Path.GetFileName(recFileName), null), ".jpg");

        if (!File.Exists(thumbNail))
        {                    
          try
          {
            if (VideoThumbCreator.VideoThumbCreator.CreateVideoThumb(recFileName, thumbNail, true))
            {
              Log.Info("ThumbProcessor - Thumbnail successfully created for - {0}", recFileName);
            }
            else
            {
              Log.Info("ThumbProcessor - No thumbnail created for - {0}", recFileName);
            }            
          }
          catch (Exception ex)
          {
            Log.Error("ThumbProcessor - No thumbnail created for {0} - {1}", recFileName, ex.Message);
          }
        }      
      }
      catch(Exception ex)
      {
        Log.Error("ThumbProcessor - No thumbnail created for {0} - {1}", recFileName, ex.Message);
      }
    }

    private void OnTvServerEvent(object sender, EventArgs eventargs)
    {
      TvServerEventArgs tvEvent = (TvServerEventArgs)eventargs;

      if (tvEvent.EventType == TvServerEventType.RecordingEnded)
      {
        _queue.EnqueueTask(new List<string> { tvEvent.Recording.FileName });
      }   
    }
  }
}
