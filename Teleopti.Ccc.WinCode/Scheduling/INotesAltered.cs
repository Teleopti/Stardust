﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public interface INotesAltered
    {
        void NotesAltered();
        bool NotesIsAltered { get; set; }
        void NoteRemoved();
    }
}
