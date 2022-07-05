using DiffPlex.DiffBuilder.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DiffPlex.Wpf.Controls
{

    internal class LineViewerLineData : INotifyPropertyChanged
    {
        private Visibility visible;

        //int? number, string operation, List<KeyValuePair<string, string>> value, string changeType, UIElement source
        //int? number, string operation, string value, string changeType, UIElement source

        public int? Number { get; set; }
        public string Operation
        {
            get
            {
                switch (ChangeType)
                {
                    case ChangeType.Inserted: return "+";
                    case ChangeType.Deleted: return "-";
                }
                return "";
            }
        }
        public List<LineViewerSegment> Segments { get; set; }

        public DiffPiece Piece { get; set; }


        public ChangeType ChangeType { get; set; }
        //TODO: Use styles properly instead of calculating color using this
        public UIElement Source { get; set; }


        public Brush FallbackForeground { get; internal set; } = null;

        public Brush ChangeBackground
        {
            get
            {
                if (Source is DiffViewer)
                {
                    switch (ChangeType)
                    {
                        case ChangeType.Inserted:
                            return (SolidColorBrush)Source.GetValue(DiffViewer.InsertedBackgroundProperty);
                        case ChangeType.Deleted:
                            return (SolidColorBrush)Source.GetValue(DiffViewer.DeletedBackgroundProperty);
                        case ChangeType.Unchanged:
                            return (SolidColorBrush)Source.GetValue(DiffViewer.UnchangedBackgroundProperty);
                    }
                }
                else if (Source is InlineDiffViewer)
                {
                    switch (ChangeType)
                    {
                        case ChangeType.Inserted:
                            return (SolidColorBrush)Source.GetValue(InlineDiffViewer.InsertedBackgroundProperty);
                        case ChangeType.Deleted:
                            return (SolidColorBrush)Source.GetValue(InlineDiffViewer.DeletedBackgroundProperty);
                        case ChangeType.Unchanged:
                            return (SolidColorBrush)Source.GetValue(InlineDiffViewer.UnchangedBackgroundProperty);
                    }
                }
                else if (Source is SideBySideDiffViewer)
                {
                    switch (ChangeType)
                    {
                        case ChangeType.Inserted:
                            return (SolidColorBrush)Source.GetValue(SideBySideDiffViewer.InsertedBackgroundProperty);
                        case ChangeType.Deleted:
                            return (SolidColorBrush)Source.GetValue(SideBySideDiffViewer.DeletedBackgroundProperty);
                        case ChangeType.Unchanged:
                            return (SolidColorBrush)Source.GetValue(SideBySideDiffViewer.UnchangedBackgroundProperty);
                    }
                }

                return null;
            }
        }

        public Brush NumberForeground
        {
            get
            {
                SolidColorBrush brushColor = null;

                if (Source is DiffViewer)
                {
                    brushColor = (SolidColorBrush)Source.GetValue(DiffViewer.LineNumberForegroundProperty);
                }
                else if (Source is InlineDiffViewer)
                {
                    brushColor = (SolidColorBrush)Source.GetValue(InlineDiffViewer.LineNumberForegroundProperty);
                }
                else if (Source is SideBySideDiffViewer)
                {
                    brushColor = (SolidColorBrush)Source.GetValue(SideBySideDiffViewer.LineNumberForegroundProperty);
                }

                return brushColor ?? FallbackForeground;
            }
        }

        public Brush ChangeForeground
        {
            get
            {
                SolidColorBrush brushColor = null;

                if (Source is DiffViewer)
                {
                    switch (ChangeType)
                    {
                        case ChangeType.Inserted:
                            brushColor = (SolidColorBrush)Source.GetValue(DiffViewer.InsertedForegroundProperty);
                            break;
                        case ChangeType.Deleted:
                            brushColor = (SolidColorBrush)Source.GetValue(DiffViewer.DeletedForegroundProperty);
                            break;
                        case ChangeType.Unchanged:
                            brushColor = (SolidColorBrush)Source.GetValue(DiffViewer.UnchangedForegroundProperty);
                            break;
                        case ChangeType.Modified:
                            brushColor = (SolidColorBrush)Source.GetValue(DiffViewer.ChangeTypeForegroundProperty);
                            break;
                    }
                }
                else if (Source is InlineDiffViewer)
                {
                    switch (ChangeType)
                    {
                        case ChangeType.Inserted:
                            brushColor = (SolidColorBrush)Source.GetValue(InlineDiffViewer.InsertedForegroundProperty);
                            break;
                        case ChangeType.Deleted:
                            brushColor = (SolidColorBrush)Source.GetValue(InlineDiffViewer.DeletedForegroundProperty);
                            break;
                        case ChangeType.Unchanged:
                            brushColor = (SolidColorBrush)Source.GetValue(InlineDiffViewer.UnchangedForegroundProperty);
                            break;
                        case ChangeType.Modified:
                            brushColor = (SolidColorBrush)Source.GetValue(InlineDiffViewer.ChangeTypeForegroundProperty);
                            break;
                    }
                }
                else if (Source is SideBySideDiffViewer)
                {
                    switch (ChangeType)
                    {
                        case ChangeType.Inserted:
                            brushColor = (SolidColorBrush)Source.GetValue(SideBySideDiffViewer.InsertedForegroundProperty);
                            break;
                        case ChangeType.Deleted:
                            brushColor = (SolidColorBrush)Source.GetValue(SideBySideDiffViewer.DeletedForegroundProperty);
                            break;
                        case ChangeType.Unchanged:
                            brushColor = (SolidColorBrush)Source.GetValue(SideBySideDiffViewer.UnchangedForegroundProperty);
                            break;
                        case ChangeType.Modified:
                            brushColor = (SolidColorBrush)Source.GetValue(SideBySideDiffViewer.ChangeTypeForegroundProperty);
                            break;
                    }
                }

                return brushColor ?? FallbackForeground;
            }
        }

        public Visibility Visible
        {
            get => visible;
            internal set
            {
                visible = value;
                RaisePropertyChanged(nameof(Visible));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class LineViewerSegment
    {
        public string TextChunk { get; set; }
        public ChangeType ChunkChange { get; set; }



        public UIElement Source { get; set; }
        public Brush FallbackForeground { get; internal set; } = null;

        public Brush SegmentBackground
        {
            get
            {
                if (Source is DiffViewer)
                {
                    switch (ChunkChange)
                    {
                        case ChangeType.Inserted:
                            return (SolidColorBrush)Source.GetValue(DiffViewer.InsertedBackgroundProperty);
                        case ChangeType.Deleted:
                            return (SolidColorBrush)Source.GetValue(DiffViewer.DeletedBackgroundProperty);
                        case ChangeType.Unchanged:
                            return (SolidColorBrush)Source.GetValue(DiffViewer.UnchangedBackgroundProperty);
                    }
                }
                else if (Source is InlineDiffViewer)
                {
                    switch (ChunkChange)
                    {
                        case ChangeType.Inserted:
                            return (SolidColorBrush)Source.GetValue(InlineDiffViewer.InsertedBackgroundProperty);
                        case ChangeType.Deleted:
                            return (SolidColorBrush)Source.GetValue(InlineDiffViewer.DeletedBackgroundProperty);
                        case ChangeType.Unchanged:
                            return (SolidColorBrush)Source.GetValue(InlineDiffViewer.UnchangedBackgroundProperty);
                    }
                }
                else if (Source is SideBySideDiffViewer)
                {
                    switch (ChunkChange)
                    {
                        case ChangeType.Inserted:
                            return (SolidColorBrush)Source.GetValue(SideBySideDiffViewer.InsertedBackgroundProperty);
                        case ChangeType.Deleted:
                            return (SolidColorBrush)Source.GetValue(SideBySideDiffViewer.DeletedBackgroundProperty);
                        case ChangeType.Unchanged:
                            return (SolidColorBrush)Source.GetValue(SideBySideDiffViewer.UnchangedBackgroundProperty);
                    }
                }

                return null;
            }
        }
        public Brush SegmentForeground
        {
            get
            {
                if (string.IsNullOrEmpty(TextChunk))
                    return null;

                SolidColorBrush brushColor = null;

                if (Source is DiffViewer)
                {
                    switch (ChunkChange)
                    {
                        case ChangeType.Inserted:
                            brushColor = (SolidColorBrush)Source.GetValue(DiffViewer.InsertedForegroundProperty);
                            break;
                        case ChangeType.Deleted:
                            brushColor = (SolidColorBrush)Source.GetValue(DiffViewer.DeletedForegroundProperty);
                            break;
                        case ChangeType.Unchanged:
                            brushColor = (SolidColorBrush)Source.GetValue(DiffViewer.UnchangedForegroundProperty);
                            break;
                        case ChangeType.Modified:
                            brushColor = (SolidColorBrush)Source.GetValue(DiffViewer.ChangeTypeForegroundProperty);
                            break;
                    }
                }
                else if (Source is InlineDiffViewer)
                {
                    switch (ChunkChange)
                    {
                        case ChangeType.Inserted:
                            brushColor = (SolidColorBrush)Source.GetValue(InlineDiffViewer.InsertedForegroundProperty);
                            break;
                        case ChangeType.Deleted:
                            brushColor = (SolidColorBrush)Source.GetValue(InlineDiffViewer.DeletedForegroundProperty);
                            break;
                        case ChangeType.Unchanged:
                            brushColor = (SolidColorBrush)Source.GetValue(InlineDiffViewer.UnchangedForegroundProperty);
                            break;
                        case ChangeType.Modified:
                            brushColor = (SolidColorBrush)Source.GetValue(InlineDiffViewer.ChangeTypeForegroundProperty);
                            break;
                    }
                }
                else if (Source is SideBySideDiffViewer)
                {
                    switch (ChunkChange)
                    {
                        case ChangeType.Inserted:
                            brushColor = (SolidColorBrush)Source.GetValue(SideBySideDiffViewer.InsertedForegroundProperty);
                            break;
                        case ChangeType.Deleted:
                            brushColor = (SolidColorBrush)Source.GetValue(SideBySideDiffViewer.DeletedForegroundProperty);
                            break;
                        case ChangeType.Unchanged:
                            brushColor = (SolidColorBrush)Source.GetValue(SideBySideDiffViewer.UnchangedForegroundProperty);
                            break;
                        case ChangeType.Modified:
                            brushColor = (SolidColorBrush)Source.GetValue(SideBySideDiffViewer.ChangeTypeForegroundProperty);
                            break;
                    }
                }

                return brushColor ?? FallbackForeground;
            }
        }
    }
}
