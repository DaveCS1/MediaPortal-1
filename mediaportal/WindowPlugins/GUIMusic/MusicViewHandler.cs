#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Soap;
using System.Xml.Serialization;
using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.View;
using MediaPortal.Music.Database;
using SQLite.NET;

namespace MediaPortal.GUI.Music
{
  /// <summary>
  /// Summary description for MusicViewHandler.
  /// </summary>
  public class MusicViewHandler : ViewHandlerNew
  {
    private readonly string _defaultMusicViews = Path.Combine(DefaultsDirectory, "MusicViews.xml");
    private readonly string _customMusicViews = Config.GetFile(Config.Dir.Config, "MusicViewsNew.xml");

    private int _previousLevel = 0;

    private readonly MusicDatabase _database;
    private int _restrictionLength = 0; // used to sum up the length of all restrictions

    private Song _currentSong = null; // holds the current Song selected in the list

    public int PreviousLevel
    {
      get { return _previousLevel; }
    }

    public override string CurrentView
    {
      get
      {
        return base.CurrentView;
      }
      set
      {
        _previousLevel = -1;
        base.CurrentView = value;
      }
    }

    public MusicViewHandler()
    {
      if (!File.Exists(_customMusicViews))
      {
        File.Copy(_defaultMusicViews, _customMusicViews);
      }

      try
      {
        using (FileStream fileStream = new FileInfo(_customMusicViews).OpenRead())
        {
          XmlSerializer serializer = new XmlSerializer(typeof(List<ViewDefinitionNew>));
          views = (List<ViewDefinitionNew>)serializer.Deserialize(fileStream);
          fileStream.Close();
        }
      }
      catch (Exception) { }

      _database = MusicDatabase.Instance;
    }

    public void Restore(ViewDefinitionNew view, int level)
    {
      currentView = view;
      currentLevel = level;
    }

    public ViewDefinitionNew GetView()
    {
      return currentView;
    }


    public void Select(Song song)
    {
      if (currentView.Levels.Count == 0)
      {
        CurrentView = song.Title;
      }
      else
      {
        FilterLevel level = (FilterLevel)currentView.Levels[CurrentLevel];
        level.SelectedValue = GetFieldValue(song, level.Selection).ToString();
        if (currentLevel < currentView.Levels.Count)
        {
          currentLevel++;
        }
      }
      _currentSong = song;
    }

    public bool Execute(out List<Song> songs)
    {
      if (currentLevel < 0)
      {
        _previousLevel = -1;
        songs = new List<Song>();
        return false;
      }

      songs = new List<Song>();

      // We're on a root view, so just list the subviews
      if (currentView.Levels.Count == 0)
      {
        foreach (ViewDefinitionNew subview in currentView.SubViews)
        {
          Song song = new Song();
          song.Title = subview.LocalizedName;
          songs.Add(song);
        }

        _previousLevel = currentLevel;
        return true;
      }

      string whereClause = string.Empty;
      string orderClause = string.Empty;
      FilterLevel level = (FilterLevel)currentView.Levels[CurrentLevel];

      _restrictionLength = 0;
      for (int i = 0; i < CurrentLevel; ++i)
      {
        BuildSelect((FilterLevel)currentView.Levels[i], ref whereClause, i);
      }
      BuildWhere((FilterLevel)currentView.Levels[CurrentLevel], ref whereClause);
      BuildOrder((FilterLevel)currentView.Levels[CurrentLevel], ref orderClause);

      if (CurrentLevel > 0)
      {
        // When grouping is active, we need to omit the "where ";
        if (!whereClause.Trim().StartsWith("group ") && whereClause.Trim() != "")
        {
          whereClause = "where " + whereClause;
        }
      }

      //execute the query
      string sql = "";
      if (CurrentLevel == 0)
      {
        FilterLevel levelRoot = currentView.Levels[0];
        string table = GetTable(levelRoot.Selection);
        string searchField = GetField(levelRoot.Selection);

        string countField = searchField; // when grouping on Albums, we need to count the artists
        // We don't have an album table anymore, so change the table to search for to tracks here.
        if (table == "album")
        {
          table = "tracks";
          countField = "strAlbumArtist";
        }

        if (levelRoot.Selection.ToLower().EndsWith("index"))
        {
          sql = string.Format("SELECT UPPER(SUBSTR({0},1,1)) AS IX, COUNT (distinct {1}) FROM {2} GROUP BY IX ", searchField, countField, table);
          _database.GetSongsByIndex(sql, out songs, CurrentLevel, table);
          
          _previousLevel = currentLevel;
          return true;  
        }

        switch (table)
        {
          case "artist":
          case "albumartist":
          case "genre":
          case "composer":
            sql = String.Format("select * from {0} ", table);
            if (whereClause != string.Empty)
            {
              sql += "where " + whereClause;
            }
            if (orderClause != string.Empty)
            {
              sql += orderClause;
            }
            break;

          case "album":
            sql = String.Format("select * from tracks ");
            if (whereClause != string.Empty)
            {
              sql += "where " + whereClause;
            }
            sql += " group by strAlbum, strAlbumArtist ";
            // We need to group on AlbumArtist, to show Albums with same name for different artists
            if (orderClause != string.Empty)
            {
              sql += orderClause;
            }
            break;

          case "tracks":
            if (levelRoot.Selection.ToLower() == "year")
            {
              songs = new List<Song>();
              sql = String.Format("select distinct iYear from tracks ");
              SQLiteResultSet results = MusicDatabase.DirectExecute(sql);
              for (int i = 0; i < results.Rows.Count; i++)
              {
                Song song = new Song();
                try
                {
                  song.Year = (int)Math.Floor(0.5d + Double.Parse(DatabaseUtility.Get(results, i, "iYear")));
                }
                catch (Exception)
                {
                  song.Year = 0;
                }
                if (song.Year > 1000)
                {
                  songs.Add(song);
                }
              }

              _previousLevel = currentLevel;

              return true;
            }
            
            if (levelRoot.Selection.ToLower() == "recently added")
            {
              try
              {
                whereClause = "";
                TimeSpan ts = new TimeSpan(7, 0, 0, 0);
                DateTime searchDate = DateTime.Today - ts;

                whereClause = String.Format("where {0} > '{1}'", searchField, searchDate.ToString("yyyy-MM-dd hh:mm:ss"));
                sql = String.Format("select * from tracks {0} {1}", whereClause, orderClause);
              }
              catch (Exception) { }
            }
            else if (levelRoot.Selection.ToLower() == "conductor")
            {
              whereClause = "";
              if (whereClause != string.Empty)
              {
                whereClause = String.Format("where {0}", whereClause);
              }
              sql = String.Format("select distinct strConductor from tracks {0} {1}", whereClause, orderClause);
            }
            else
            {
              whereClause = "";
              if (whereClause != string.Empty)
              {
                whereClause = String.Format("where {0}", whereClause);
              }
              sql = String.Format("select * from tracks {0} {1}", whereClause, orderClause);
            }
            break;
        }
        _database.GetSongsByFilter(sql, out songs, table);
      }
      else if (CurrentLevel < MaxLevels - 1)
      {
        FilterLevel defCurrent = (FilterLevel)currentView.Levels[CurrentLevel];
        string table = GetTable(defCurrent.Selection);

        // Now we need to check the previous filters, if we were already on the tracks table previously
        // In this case the from clause must contain the tracks table only
        bool isUsingTrackTable = false;
        string allPrevColumns = string.Empty;
        for (int i = CurrentLevel; i > -1; i--)
        {
          FilterLevel lvl = (FilterLevel)currentView.Levels[i];

          allPrevColumns += " " + GetField(lvl.Selection) + " ,";
          if (GetTable(lvl.Selection) != table)
          {
            isUsingTrackTable = true;
          }
        }
        allPrevColumns = allPrevColumns.Remove(allPrevColumns.Length - 1, 1); // remove extra trailing comma

        if (level.Selection.ToLower().EndsWith("index"))
        {
          // in an odd scenario here as we have a group operator
          // but not at the first level of view

          // Build correct table for search
          string searchTable = GetTable(defCurrent.Selection);
          string searchField = GetField(defCurrent.Selection);
          string countField = searchField;
          // We don't have an album table anymore, so change the table to search for to tracks here.
          if (table == "album")
          {
            searchTable = "tracks";
            countField = "strAlbumArtist";
          }

          if (isUsingTrackTable && searchTable != "tracks")
          {
            // have the messy case where previous filters in view
            // do not use the same table as the current level
            // which means we can not just lookup values in search table

            string joinSQL;
            if (IsMultipleValueField(searchField))
            {
              joinSQL = string.Format("and tracks.{1} like '%| '||{0}.{1}||' |%' ",
                                      searchTable, searchField);
            }
            else
            {
              joinSQL = string.Format("and tracks.{1} = {0}.{1} ",
                                      searchTable, searchField);
            }

            whereClause = whereClause.Replace("group by ix", "");
            whereClause = string.Format(" where exists ( " +
                                        "    select 0 " +
                                        "    from tracks " +
                                        "    {0} " +
                                        "    {1} " +
                                        ") " +
                                        "group by ix "
                                        , whereClause, joinSQL);
          }

          sql = String.Format("select UPPER(SUBSTR({0},1,1)) IX, Count(distinct {1}) from {2} {3} {4}",
                              searchField, countField, searchTable, whereClause, orderClause);
          _database.GetSongsByIndex(sql, out songs, CurrentLevel, table);
        }
        else
        {
          string from = String.Format("{1} from {0}", table, GetField(defCurrent.Selection));

          if (isUsingTrackTable && table != "album" && defCurrent.Selection != "Disc#")
          {
            from = String.Format("{0} from tracks", allPrevColumns);
            table = "tracks";
          }

          // When searching for an album, we need to retrieve the AlbumArtist as well, because we could have same album names for different artists
          // We need also the Path to retrieve the coverart
          // We don't have an album table anymore, so change the table to search for to tracks here.

          for (int i = 0; i < currentLevel; i++)
          {
            // get previous filter to see, if we had an album that was not a group level
            FilterLevel defPrevious = (FilterLevel)currentView.Levels[i];
            if (defPrevious.Selection.ToLower() == "album" && !defPrevious.Selection.ToLower().EndsWith("index"))
            {
              if (whereClause != "")
              {
                whereClause += " and ";
              }

              string selectedArtist = _currentSong.AlbumArtist;
              DatabaseUtility.RemoveInvalidChars(ref selectedArtist);

              // we don't store "unknown" into the datbase, so let's correct empty values
              if (selectedArtist == "unknown")
              {
                selectedArtist = string.Empty;
              }

              whereClause += String.Format("strAlbumArtist like '%| {0} |%'", selectedArtist);
              break;
            }
          }

          if (table == "album")
          {
            from = String.Format("* from tracks", GetField(defCurrent.Selection));
            whereClause += " group by strAlbum, strAlbumArtist ";
          }
          if (defCurrent.Selection.ToLower() == "disc#")
          {
            from = String.Format("* from tracks", GetField(defCurrent.Selection));
            whereClause += " group by strAlbum, strAlbumArtist, iDisc ";
          }

          sql = String.Format("select distinct {0} {1} {2}", from, whereClause, orderClause);

          _database.GetSongsByFilter(sql, out songs, table);
        }
      }
      else
      {
        for (int i = 0; i < currentLevel; i++)
        {
          // get previous filter to see, if we had an album that was not a group level
          FilterLevel defPrevious = (FilterLevel)currentView.Levels[i];
          if (defPrevious.Selection.ToLower() == "album" && !defPrevious.Selection.ToLower().EndsWith("index"))
          {
            if (whereClause != "")
            {
              whereClause += " and ";
            }

            string selectedArtist = _currentSong.AlbumArtist;
            DatabaseUtility.RemoveInvalidChars(ref selectedArtist);

            // we don't store "unknown" into the datbase, so let's correct empty values
            if (selectedArtist == "unknown")
            {
              selectedArtist = string.Empty;
            }

            whereClause += String.Format("strAlbumArtist like '%| {0} |%'", selectedArtist);
            break;
          }
        }

        sql = String.Format("select * from tracks {0} {1}", whereClause, orderClause);

        _database.GetSongsByFilter(sql, out songs, "tracks");
      }

      if (songs.Count == 1 && level.SkipLevel)
      {
        if (currentLevel < MaxLevels - 1)
        {
          if (_previousLevel < currentLevel)
          {
            FilterLevel fd = (FilterLevel)currentView.Levels[currentLevel];
            fd.SelectedValue = GetFieldValue(songs[0], fd.Selection);
            currentLevel = currentLevel + 1;
          }
          else
          {
            currentLevel = currentLevel - 1;
          }
          if (!Execute(out songs))
          {
            return false;
          }
        }
      }

      _previousLevel = currentLevel;

      return true;
    }

    private void BuildSelect(FilterLevel filter, ref string whereClause, int filterLevel)
    {
      if (whereClause != "")
      {
        whereClause += " and ";
      }
      string selectedValue = filter.SelectedValue;
      DatabaseUtility.RemoveInvalidChars(ref selectedValue);

      // we don't store "unknown" into the datbase, so let's correct empty values
      if (selectedValue == "unknown")
      {
        selectedValue = "";
      }

      // If we have a multiple values field then we need to compare with like
      if (IsMultipleValueField(GetField(filter.Selection)))
      {
        whereClause += String.Format("{0} like '%| {1} |%'", GetField(filter.Selection), selectedValue);
      }
      else
      {
        // use like for case insensitivity
        whereClause += String.Format("{0} like '{1}'", GetField(filter.Selection), selectedValue);
      }
    }

    private void BuildWhere(FilterLevel filter, ref string whereClause)
    {
      if (filter.SelectedValue != "*" && filter.SelectedValue != "")
      {
        if (whereClause != "")
        {
          whereClause += " and ";
        }
        string selectedValue = filter.SelectedValue;
        DatabaseUtility.RemoveInvalidChars(ref selectedValue);

        // Do we have a Multiplevalues field, then we need compare with like
        if (IsMultipleValueField(GetField(filter.Selection)))
        {
          whereClause += String.Format(" {0} like '%| {1} |%'", GetField(filter.Selection), selectedValue);
        }
        else
        {
          // use like for case insensitivity
          whereClause += String.Format(" {0} like '{1}'", GetField(filter.Selection), selectedValue);
        }
      }
    }

    private void BuildOrder(FilterLevel filter, ref string orderClause)
    {
      string[] sortFields = GetSortField(filter).Split('|');
      orderClause = " order by ";
      for (int i = 0; i < sortFields.Length; i++)
      {
        if (i > 0)
        {
          orderClause += ", ";
        }
        orderClause += sortFields[i];
        if (!filter.SortAscending)
        {
          orderClause += " desc";
        }
        else
        {
          orderClause += " asc";
        }
      }
    }

    /// <summary>
    /// Check, if this is a field with multiple values, for which we need to compare with Like %value% instead of equals
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    private static bool IsMultipleValueField(string field)
    {
      switch (field)
      {
        case "strArtist":
        case "strAlbumArtist":
        case "strGenre":
        case "strComposer":
          return true;

        default:
          return false;
      }
    }

    private static string GetTable(string selection)
    {
      switch (selection.ToLower())
      {
        case "artist":
        case "artistindex":
          return "artist";

        case "albumartist":
        case "albumartistindex":
          return "albumartist";

        case "composer":
        case "composerindex":
          return "composer";

        case "genre":
        case "genreindex":
          return "genre";

        case "album":
          return "album";

        default:
          return "tracks";
      }
    }

    private static string GetField(string selection)
    {
      switch (selection.ToLower())
      {
        case "path":
          return "strPath";

        case "artist":
        case "artistindex":
          return "strArtist";

        case "albumartist":
        case "albumartistindex":
          return "strAlbumArtist";

        case "album":
          return "strAlbum";

        case "genre":
        case "genreindex":
          return "strGenre";

        case "title":
          return "strTitle";

        case "composer":
        case "composerindex":
          return "strComposer";

        case "conductor":
        case "conductorindex":
          return "strConductor";

        case "year":
          return "strYear";

        case "track#":
          return "iTrack";

        case "numtracks":
          return "iNumTracks";

        case "timesplayed":
          return "iTimesPlayed";

        case "rating":
          return "iRating";

        case "favorites":
          return "iFavorite";

        case "dateadded":
          return "dateAdded";

        case "datelastplayed":
          return "dateLastPlayed";

        case "disc#":
          return "iDisc";

        case "numdiscs":
          return "iNumDiscs";

        case "duration":
          return "iDuration";

        case "resumeat":
          return "iResumeAt";

        case "lyrics":
          return "strLyrics";

        case "comment":
          return "strComment";

        case "filetype":
          return "strFileType";

        case "fullcodec":
          return "strFullCodec";

        case "bitratemode":
          return "strBitRateMode";

        case "bpm":
          return "iBPM";

        case "bitrate":
          return "iBitRate";

        case "channels":
          return "iChannels";

        case "samplerate":
          return "iSampleRate";
      }

      return null;
    }

    public static string GetFieldValue(Song song, string selection)
    {
      switch (selection.ToLower())
      {
        case "path":
          return Path.GetDirectoryName(song.FileName);

        case "artist":
        case "artistindex":
          return song.Artist;

        case "albumartist":
        case "albumartistindex":
          return song.AlbumArtist;

        case "album":
          return song.Album;

        case "genre":
        case "genreindex":
          return song.Genre;

        case "title":
          return song.Title;

        case "composer":
        case "composerindex":
          return song.Composer;

        case "conductor":
        case "conductorindex":
          return song.Conductor;

        case "year":
          return song.Year.ToString();

        case "track#":
          return song.Track.ToString();

        case "numtracks":
          return song.TrackTotal.ToString();

        case "timesplayed":
          return song.TimesPlayed.ToString();

        case "rating":
          return song.Rating.ToString();

        case "favorites":
          {
            if (song.Favorite)
            {
              return "1";
            }
            return "0";
          }

        case "dateadded":
          return song.DateTimeModified.ToShortDateString();

        case "datelastplayed":
          return song.DateTimePlayed.ToShortDateString();

        case "disc#":
          return song.DiscId.ToString();

        case "numdiscs":
          return song.DiscTotal.ToString();

        case "duration":
          return song.Duration.ToString();

        case "resumeat":
          return song.ResumeAt.ToString();

        case "lyrics":
          return song.Lyrics;

        case "comment":
          return song.Comment;

        case "filetype":
          return song.FileType;

        case "fullcodec":
          return song.Codec;

        case "bitratemode":
          return song.BitRateMode;

        case "bpm":
          return song.BPM.ToString();

        case "bitrate":
          return song.BitRate.ToString();

        case "channels":
          return song.Channels.ToString();

        case "samplerate":
          return song.SampleRate.ToString();
      }

      return "";
    }

    private static string GetSortField(FilterLevel filter)
    {
      // Don't allow anything else but the fieldnames itself on Multiple Fields
      if (filter.Selection.ToLower() == "artist" || filter.Selection.ToLower() == "albumartist" || 
        filter.Selection.ToLower() == "genre" || filter.Selection.ToLower() == "composer")
      {
        return GetField(filter.Selection);
      }

      if (filter.SortBy == "Date")
      {
        return GetField("dateadded");
      }
      if (filter.SortBy == "Year")
      {
        return GetField("year");
      }
      if (filter.SortBy == "Name")
      {
        return GetField("title");
      }
      if (filter.SortBy == "Duration")
      {
        return "iDuration";
      }
      if (filter.SortBy == "disc#")
      {
        return "iDisc";
      }
      if (filter.SortBy == "Track")
      {
        return "iDisc|iTrack"; // We need to sort on Discid + Track
      }
      return GetField(filter.Selection);
    }

    protected override string GetLocalizedViewLevel(string lvlName)
    {
      string localizedLevelName = string.Empty;

      switch (lvlName)
      {
        case "artist":
          localizedLevelName = GUILocalizeStrings.Get(133);
          break;
        case "albumartist":
          localizedLevelName = GUILocalizeStrings.Get(528);
          break;
        case "album":
          localizedLevelName = GUILocalizeStrings.Get(132);
          break;
        case "genre":
          localizedLevelName = GUILocalizeStrings.Get(135);
          break;
        case "year":
          localizedLevelName = GUILocalizeStrings.Get(987);
          break;
        case "composer":
          localizedLevelName = GUILocalizeStrings.Get(1214);
          break;
        case "conductor":
          localizedLevelName = GUILocalizeStrings.Get(1215);
          break;
        case "disc#":
          localizedLevelName = GUILocalizeStrings.Get(1216);
          break;
        case "title":
        case "timesplayed":
        case "rating":
        case "favorites":
        case "recently added":
        case "track":
          localizedLevelName = GUILocalizeStrings.Get(1052);
          break;
        default:
          localizedLevelName = lvlName;
          break;
      }

      return localizedLevelName;
    }

  }
}