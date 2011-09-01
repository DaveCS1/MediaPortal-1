/*
 *  Copyright (C) 2005-2011 Team MediaPortal
 *  http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#pragma once

#include "Packet.h"
#include <map>
#include <vector>
#include <dshow.h>

using namespace std;

// TODO - enum
#define SUPERCEEDED_AUDIO    1
#define SUPERCEEDED_VIDEO    2
#define SUPERCEEDED_SUBTITLE 4

#define FAKE_AUDIO_DURATION 320000LL
#define AC3_FRAME_LENGTH 896
#define AC3_HEADER_LENGHT 224

// Silent AC3 frame
static unsigned char ac3_224k2_48[AC3_HEADER_LENGHT] = {
  0x0B, 0x77, 0x5E, 0xC1, 0x16, 0x40, 0x53, 0xE1, 0x06, 0xA0, 0xB9, 0x65, 0x9D, 0xEE, 0x5B, 0xA4, 
  0xAA, 0xB4, 0x77, 0xAA, 0xA1, 0x56, 0xAE, 0x99, 0x4C, 0x2A, 0x74, 0xA1, 0x42, 0x7B, 0x99, 0xF4, 
  0x2A, 0xEF, 0x5D, 0x3E, 0x7E, 0xF9, 0xF5, 0x77, 0xCA, 0x94, 0xBE, 0xA4, 0xF9, 0xFA, 0x5A, 0xF0, 
  0xA9, 0xB9, 0x72, 0xF9, 0xFB, 0xE4, 0xAF, 0x9F, 0x42, 0xB4, 0xFD, 0x33, 0xDB, 0x04, 0x5D, 0xA5, 
  0x7C, 0xAA, 0x92, 0x9B, 0x09, 0x88, 0x43, 0x7C, 0xF6, 0x9B, 0xF4, 0xAE, 0x62, 0x1B, 0xDE, 0xDB, 
  0xEA, 0xEF, 0x9D, 0xAA, 0x73, 0x49, 0xF2, 0xAA, 0xF5, 0x9F, 0xA5, 0x7C, 0xA9, 0xD2, 0xB7, 0xC9, 
  0x5F, 0x3E, 0x7C, 0xFD, 0x52, 0x9A, 0xEE, 0x6C, 0x21, 0x52, 0xFD, 0xCA, 0xEB, 0x09, 0x5D, 0x3F, 
  0x49, 0x9A, 0x20, 0xCA, 0xE9, 0x5F, 0xC3, 0xAA, 0xFD, 0x0B, 0xF4, 0xCE, 0x69, 0xD1, 0x7C, 0xFA, 
  0x01, 0x0A, 0xD0, 0xCA, 0xD1, 0xD7, 0x46, 0xDC, 0x2A, 0xCF, 0x9C, 0xD6, 0xA7, 0x61, 0xC1, 0x96, 
  0x9F, 0xFE, 0x7C, 0x3F, 0xDF, 0xFF, 0x6E, 0xDA, 0xDF, 0xFF, 0xC0, 0xDA, 0xD3, 0x00, 0x0B, 0x3D, 
  0x0D, 0x42, 0x8B, 0xBA, 0x1F, 0x0A, 0xA3, 0xC1, 0x80, 0x05, 0x86, 0x56, 0x1B, 0xC0, 0xC5, 0x13, 
  0x66, 0x45, 0x30, 0xC0, 0x02, 0xA3, 0x53, 0x10, 0xAB, 0x62, 0x89, 0xB3, 0x22, 0x98, 0x60, 0x01, 
  0x51, 0xA5, 0x98, 0x55, 0x4E, 0x40, 0xCD, 0x29, 0x4D, 0x30, 0x00, 0x9E, 0xD2, 0xD3, 0x7A, 0xA7, 
  0x1C, 0x70, 0xA8, 0xA6, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};

class CClip
{
public:
  CClip(int clipNumber, REFERENCE_TIME playlistFirstPacketTime, REFERENCE_TIME clipOffset, bool audioPresent, REFERENCE_TIME duration, bool seekNeeded);
  ~CClip(void);
  Packet* ReturnNextAudioPacket(REFERENCE_TIME playlistOffset);
  Packet* ReturnNextVideoPacket(REFERENCE_TIME playlistOffset);
  bool AcceptAudioPacket(Packet*  packet, bool forced);
  bool AcceptVideoPacket(Packet*  packet, bool forced);
  void FlushAudio(void);
  void FlushVideo(void);
  int  nClip;
  bool noAudio;
  bool clipFilled;
  bool clipEmptied;
  void Superceed(int superceedType);
  bool IsSuperceeded(int superceedType);
  REFERENCE_TIME playlistFirstPacketTime;
  REFERENCE_TIME clipPlaylistOffset;
  int AudioPacketCount();
  int VideoPacketCount();
  void Reset();
  bool FakeAudioAvailable();
  bool HasAudio();
  bool HasVideo();
  bool Incomplete();
  void SetPMT(AM_MEDIA_TYPE *pmt);

protected:
  typedef vector<Packet*>::iterator ivecVideoBuffers;
  typedef vector<Packet*>::iterator ivecAudioBuffers;
  vector<Packet*> m_vecClipAudioPackets;
  vector<Packet*> m_vecClipVideoPackets;
  AM_MEDIA_TYPE *m_pmt;
  REFERENCE_TIME clipDuration;
  REFERENCE_TIME audioPlaybackpoint;
  REFERENCE_TIME lastAudioPosition;
  REFERENCE_TIME lastVideoPosition;
  int superceeded;

  bool firstAudio;
  bool firstVideo;
  bool bSeekNeeded;

  Packet* GenerateFakeAudio(REFERENCE_TIME rtStart);
};

