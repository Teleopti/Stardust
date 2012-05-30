using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Teleopti.Analytics.Etl.ConfigTool.Gui.SpinningProgress
{
    public partial class SpinningProgressControl  
    { 
        private Color _inactiveColor = Color.FromArgb( 218, 218, 218 ); 
        private Color _activeColor = Color.FromArgb( 35, 146, 33 ); 
        private Color _transissionColor = Color.FromArgb( 129, 242, 121 ); 
        
        private Region _innerBackgroundRegion; 
        private readonly System.Drawing.Drawing2D.GraphicsPath[] _segmentPaths = new System.Drawing.Drawing2D.GraphicsPath[ 12 ]; 
        
        private bool _autoIncrement = true; 
        private double _incrementFrequency = 100; 
        private bool _behindIsActive = true; 
        private int _transitionSegment; 
        
        private System.Timers.Timer _autoRotateTimer;
        [System.ComponentModel.DefaultValue(typeof(Color), "218, 218, 218")]
        public Color InactiveSegmentColor 
        { 
            get 
            { 
                return _inactiveColor; 
            } 
            set 
            { 
                _inactiveColor = value; 
                Invalidate(); 
            } 
        }
        [System.ComponentModel.DefaultValue(typeof(Color), "35, 146, 33")]
        public Color ActiveSegmentColor 
        { 
            get 
            { 
                return _activeColor; 
            } 
            set 
            { 
                _activeColor = value; 
                Invalidate(); 
            } 
        } 
        [ System.ComponentModel.DefaultValue( typeof( Color ), "129, 242, 121" ) ]
        public Color TransitionSegmentColor 
        { 
            get 
            { 
                return _transissionColor; 
            } 
            set 
            { 
                _transissionColor = value; 
                Invalidate(); 
            } 
        }
        [System.ComponentModel.DefaultValue(true)]
        public bool BehindTransitionSegmentIsActive 
        { 
            get 
            { 
                return _behindIsActive; 
            } 
            set 
            { 
                _behindIsActive = value; 
                Invalidate(); 
            } 
        }
        [System.ComponentModel.DefaultValue(-1)]
        public int TransitionSegment 
        { 
            get 
            { 
                return _transitionSegment; 
            } 
            set 
            { 
                if ( value > 11 || value < -1 ) 
                { 
                    throw new ArgumentException( "TransistionSegment must be between -1 and 11" ); 
                } 
                _transitionSegment = value; 
                Invalidate(); 
            } 
        }
        [System.ComponentModel.DefaultValue(true)]
        public bool AutoIncrement 
        { 
            get 
            { 
                return _autoIncrement; 
            } 
            set 
            { 
                _autoIncrement = value; 
                
                if ( value == false && _autoRotateTimer != null ) 
                { 
                    _autoRotateTimer.Dispose(); 
                    _autoRotateTimer = null; 
                } 
                
                if ( value && _autoRotateTimer == null ) 
                { 
                    _autoRotateTimer = new System.Timers.Timer( _incrementFrequency ); 
                    _autoRotateTimer.Elapsed += new System.Timers.ElapsedEventHandler( IncrementTransisionSegment ); 
                    _autoRotateTimer.Start(); 
                } 
            } 
        }
        [System.ComponentModel.DefaultValue(100)]
        public double AutoIncrementFrequency 
        { 
            get 
            { 
                return _incrementFrequency; 
            } 
            set 
            { 
                _incrementFrequency = value; 
                
                if ( _autoRotateTimer != null ) 
                { 
                    AutoIncrement = false; 
                    AutoIncrement = true; 
                } 
            } 
        } 
        
        public SpinningProgressControl() 
        { 
            //  This call is required by the Windows Form Designer.
            InitializeComponent(); 
            
            //  Add any initialization after the InitializeComponent() call.
            CalculateSegments(); 
            
            _autoRotateTimer = new System.Timers.Timer( _incrementFrequency ); 
            _autoRotateTimer.Elapsed += new System.Timers.ElapsedEventHandler( IncrementTransisionSegment );
            DoubleBuffered = true;
            _autoRotateTimer.Start(); 

            EnabledChanged += new EventHandler( SpinningProgress_EnabledChanged ); 
            // events handled by ProgressDisk_Paint
            Paint += new PaintEventHandler(ProgressDisk_Paint); 
            // events handled by ProgressDisk_Resize
            Resize += new EventHandler( ProgressDisk_Resize); 
            // events handled by ProgressDisk_SizeChanged
            SizeChanged += new EventHandler(ProgressDisk_SizeChanged); 
        } 
        
        private void IncrementTransisionSegment( object sender, System.Timers.ElapsedEventArgs e ) 
        { 
            if ( _transitionSegment == 11 ) 
            { 
                _transitionSegment = 0; 
                _behindIsActive = !( _behindIsActive ); 
            } 
            else if ( _transitionSegment == -1 ) 
            { 
                _transitionSegment = 0; 
            } 
            else 
            { 
                _transitionSegment += 1; 
            } 
            
            Invalidate(); 
        } 
        
        
        private void CalculateSegments() 
        { 
            var rctFull = new Rectangle( 0, 0, Width, Height ); 
            var rctInner = new Rectangle( ((Width *  7) / 30),
                                                ((Height *  7) / 30),
                                                (Width -  ((Width *  14 ) / 30 )),
                                                (Height - ((Height * 14) / 30 )));

        	// Create 12 segment pieces
            for ( int intCount=0; intCount < 12; intCount++ ) 
            { 
                _segmentPaths[ intCount ] = new GraphicsPath(); 
                
                // We subtract 90 so that the starting segment is at 12 o'clock
                _segmentPaths[ intCount ].AddPie( rctFull, ( intCount * 30 ) - 90, 25 ); 
            } 
            
            // Create the center circle cut-out
            var pthInnerBackground = new GraphicsPath(); 
            pthInnerBackground.AddPie( rctInner, 0, 360 ); 
            _innerBackgroundRegion = new Region( pthInnerBackground ); 
        } 
        
        
        private void SpinningProgress_EnabledChanged( object sender, EventArgs e ) 
        { 
            if ( Enabled ) 
            { 
                if ( _autoRotateTimer != null ) 
                { 
                    _autoRotateTimer.Start(); 
                } 
            } 
            else 
            { 
                if ( _autoRotateTimer != null ) 
                { 
                    _autoRotateTimer.Stop(); 
                } 
            } 
        } 
        
        
        private void ProgressDisk_Paint( object sender, PaintEventArgs e ) 
        { 
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias; 
            e.Graphics.ExcludeClip( _innerBackgroundRegion ); 
            
            for ( int intCount=0; intCount < 12; intCount++ ) 
            { 
                if ( Enabled ) 
                { 
                    if ( intCount == _transitionSegment ) 
                    { 
                        // If this segment is the transistion segment, colour it differently
                        using(var brush = new SolidBrush( _transissionColor ))
                        {
                            e.Graphics.FillPath(brush, _segmentPaths[intCount]);
                        }
                    } 
                    else if ( intCount < _transitionSegment ) 
                    { 
                        // This segment is behind the transistion segment
                        if ( _behindIsActive ) 
                        { 
                            // If behind the transistion should be active, 
                            // colour it with the active colour
                            using (var brush = new SolidBrush(_activeColor))
                            {
                                e.Graphics.FillPath(brush, _segmentPaths[intCount]);
                            }
                        } 
                        else 
                        { 
                            // If behind the transistion should be in-active, 
                            // colour it with the in-active colour
                            using (var brush = new SolidBrush(_inactiveColor))
                            {
                                e.Graphics.FillPath(brush, _segmentPaths[intCount]);
                            }
                        } 
                    } 
                    else 
                    { 
                        // This segment is ahead of the transistion segment
                        if ( _behindIsActive ) 
                        { 
                            // If behind the the transistion should be active, 
                            // colour it with the in-active colour
                            using (var brush = new SolidBrush(_inactiveColor))
                            {
                                e.Graphics.FillPath(brush, _segmentPaths[intCount]);
                            }
                        } 
                        else 
                        { 
                            // If behind the the transistion should be in-active, 
                            // colour it with the active colour
                            using (var brush = new SolidBrush(_activeColor))
                            {
                                e.Graphics.FillPath(brush, _segmentPaths[intCount]);
                            }
                        } 
                    } 
                } 
                else 
                { 
                    // Draw all segments in in-active colour if not enabled
                    using (var brush = new SolidBrush(_inactiveColor))
                    {
                        e.Graphics.FillPath(brush, _segmentPaths[intCount]);
                    }
                } 
            } 
        } 
        
        
        private void ProgressDisk_Resize( object sender, EventArgs e ) 
        { 
            CalculateSegments(); 
        } 
        
        
        private void ProgressDisk_SizeChanged( object sender, EventArgs e ) 
        { 
            CalculateSegments(); 
        } 
        
    }
}