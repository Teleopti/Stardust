using System;
using System.Drawing;
using System.Windows.Forms;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.SpinningProgress
{
    public partial class SpinningProgressControl  
    { 
        private Color m_InactiveColor = Color.FromArgb( 218, 218, 218 ); 
        private Color m_ActiveColor = Color.FromArgb( 35, 146, 33 ); 
        private Color m_TransissionColor = Color.FromArgb( 129, 242, 121 ); 
        
        private Region innerBackgroundRegion; 
        private System.Drawing.Drawing2D.GraphicsPath[] segmentPaths = new System.Drawing.Drawing2D.GraphicsPath[ 12 ]; 
        
        private bool m_AutoIncrement = true; 
        private double m_IncrementFrequency = 100; 
        private bool m_BehindIsActive = true; 
        private int m_TransitionSegment; 
        
        private System.Timers.Timer m_AutoRotateTimer;
        [System.ComponentModel.DefaultValue(typeof(Color), "218, 218, 218")]
        public Color InactiveSegmentColor 
        { 
            get 
            { 
                return m_InactiveColor; 
            } 
            set 
            { 
                m_InactiveColor = value; 
                Invalidate(); 
            } 
        }
        [System.ComponentModel.DefaultValue(typeof(Color), "35, 146, 33")]
        public Color ActiveSegmentColor 
        { 
            get 
            { 
                return m_ActiveColor; 
            } 
            set 
            { 
                m_ActiveColor = value; 
                Invalidate(); 
            } 
        } 
        [ System.ComponentModel.DefaultValue( typeof( Color ), "129, 242, 121" ) ]
        public Color TransitionSegmentColor 
        { 
            get 
            { 
                return m_TransissionColor; 
            } 
            set 
            { 
                m_TransissionColor = value; 
                Invalidate(); 
            } 
        }
        [System.ComponentModel.DefaultValue(true)]
        public bool BehindTransitionSegmentIsActive 
        { 
            get 
            { 
                return m_BehindIsActive; 
            } 
            set 
            { 
                m_BehindIsActive = value; 
                Invalidate(); 
            } 
        }
        [System.ComponentModel.DefaultValue(-1)]
        public int TransitionSegment 
        { 
            get 
            { 
                return m_TransitionSegment; 
            } 
            set 
            { 
                if ( value > 11 || value < -1 ) 
                { 
                    throw new ArgumentException( "TransistionSegment must be between -1 and 11" ); 
                } 
                m_TransitionSegment = value; 
                Invalidate(); 
            } 
        }
        [System.ComponentModel.DefaultValue(true)]
        public bool AutoIncrement 
        { 
            get 
            { 
                return m_AutoIncrement; 
            } 
            set 
            { 
                m_AutoIncrement = value; 
                
                if ( value == false && m_AutoRotateTimer != null ) 
                { 
                    m_AutoRotateTimer.Dispose(); 
                    m_AutoRotateTimer = null; 
                } 
                
                if ( value == true && m_AutoRotateTimer == null ) 
                { 
                    m_AutoRotateTimer = new System.Timers.Timer( m_IncrementFrequency ); 
                    m_AutoRotateTimer.Elapsed += new System.Timers.ElapsedEventHandler( IncrementTransisionSegment ); 
                    m_AutoRotateTimer.Start(); 
                } 
            } 
        }
        [System.ComponentModel.DefaultValue(100)]
        public double AutoIncrementFrequency 
        { 
            get 
            { 
                return m_IncrementFrequency; 
            } 
            set 
            { 
                m_IncrementFrequency = value; 
                
                if ( m_AutoRotateTimer != null ) 
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
            
            m_AutoRotateTimer = new System.Timers.Timer( m_IncrementFrequency ); 
            m_AutoRotateTimer.Elapsed += new System.Timers.ElapsedEventHandler( IncrementTransisionSegment );
            this.DoubleBuffered = true;
            m_AutoRotateTimer.Start(); 

            this.EnabledChanged += new EventHandler( SpinningProgress_EnabledChanged ); 
            // events handled by ProgressDisk_Paint
            this.Paint += new PaintEventHandler(ProgressDisk_Paint); 
            // events handled by ProgressDisk_Resize
            this.Resize += new EventHandler( ProgressDisk_Resize); 
            // events handled by ProgressDisk_SizeChanged
            this.SizeChanged += new EventHandler(ProgressDisk_SizeChanged); 
        } 
        
        private void IncrementTransisionSegment( object sender, System.Timers.ElapsedEventArgs e ) 
        { 
            if ( m_TransitionSegment == 11 ) 
            { 
                m_TransitionSegment = 0; 
                m_BehindIsActive = !( m_BehindIsActive ); 
            } 
            else if ( m_TransitionSegment == -1 ) 
            { 
                m_TransitionSegment = 0; 
            } 
            else 
            { 
                m_TransitionSegment += 1; 
            } 
            
            Invalidate(); 
        } 
        
        
        private void CalculateSegments() 
        { 
            Rectangle rctFull = new Rectangle( 0, 0, Width, Height ); 
            Rectangle rctInner = new Rectangle( ((Width *  7) / 30),
                                                ((Height *  7) / 30),
                                                (Width -  ((Width *  14 ) / 30 )),
                                                (Height - ((Height * 14) / 30 ))); 

            System.Drawing.Drawing2D.GraphicsPath pthInnerBackground; 
            
            // Create 12 segment pieces
            for ( int intCount=0; intCount < 12; intCount++ ) 
            { 
                segmentPaths[ intCount ] = new System.Drawing.Drawing2D.GraphicsPath(); 
                
                // We subtract 90 so that the starting segment is at 12 o'clock
                segmentPaths[ intCount ].AddPie( rctFull, ( intCount * 30 ) - 90, 25 ); 
            } 
            
            // Create the center circle cut-out
            pthInnerBackground = new System.Drawing.Drawing2D.GraphicsPath(); 
            pthInnerBackground.AddPie( rctInner, 0, 360 ); 
            innerBackgroundRegion = new Region( pthInnerBackground ); 
        } 
        
        
        private void SpinningProgress_EnabledChanged( object sender, EventArgs e ) 
        { 
            if ( Enabled ) 
            { 
                if ( m_AutoRotateTimer != null ) 
                { 
                    m_AutoRotateTimer.Start(); 
                } 
            } 
            else 
            { 
                if ( m_AutoRotateTimer != null ) 
                { 
                    m_AutoRotateTimer.Stop(); 
                } 
            } 
        } 
        
        
        private void ProgressDisk_Paint( object sender, PaintEventArgs e ) 
        { 
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; 
            e.Graphics.ExcludeClip( innerBackgroundRegion ); 
            
            for ( int intCount=0; intCount < 12; intCount++ ) 
            { 
                if ( Enabled ) 
                { 
                    if ( intCount == m_TransitionSegment ) 
                    { 
                        // If this segment is the transistion segment, colour it differently
                        using(var brush = new SolidBrush( m_TransissionColor ))
                        {
                            e.Graphics.FillPath(brush, segmentPaths[intCount]);
                        }
                    } 
                    else if ( intCount < m_TransitionSegment ) 
                    { 
                        // This segment is behind the transistion segment
                        if ( m_BehindIsActive ) 
                        { 
                            // If behind the transistion should be active, 
                            // colour it with the active colour
                            using (var brush = new SolidBrush(m_ActiveColor))
                            {
                                e.Graphics.FillPath(brush, segmentPaths[intCount]);
                            }
                        } 
                        else 
                        { 
                            // If behind the transistion should be in-active, 
                            // colour it with the in-active colour
                            using (var brush = new SolidBrush(m_InactiveColor))
                            {
                                e.Graphics.FillPath(brush, segmentPaths[intCount]);
                            }
                        } 
                    } 
                    else 
                    { 
                        // This segment is ahead of the transistion segment
                        if ( m_BehindIsActive ) 
                        { 
                            // If behind the the transistion should be active, 
                            // colour it with the in-active colour
                            using (var brush = new SolidBrush(m_InactiveColor))
                            {
                                e.Graphics.FillPath(brush, segmentPaths[intCount]);
                            }
                        } 
                        else 
                        { 
                            // If behind the the transistion should be in-active, 
                            // colour it with the active colour
                            using (var brush = new SolidBrush(m_ActiveColor))
                            {
                                e.Graphics.FillPath(brush, segmentPaths[intCount]);
                            }
                        } 
                    } 
                } 
                else 
                { 
                    // Draw all segments in in-active colour if not enabled
                    using (var brush = new SolidBrush(m_InactiveColor))
                    {
                        e.Graphics.FillPath(brush, segmentPaths[intCount]);
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