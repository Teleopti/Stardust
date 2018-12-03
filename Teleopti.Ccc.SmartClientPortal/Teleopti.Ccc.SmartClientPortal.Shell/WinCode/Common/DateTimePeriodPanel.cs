using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.CustomPanels.Grouping;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Panels;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
    public class DateTimePeriodPanel : BaseGroupingPanel
    {

        #region Dependencyproperties

        #region ClipPeriodProperty
        /// <summary>
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-02-27
        /// </remarks>
        public static bool GetClipPeriod(DependencyObject obj)
        {
            return (bool)obj.GetValue(ClipPeriodProperty);
        }

        /// <summary>
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-02-27
        /// </remarks>
        public static void SetClipPeriod(DependencyObject obj, bool value)
        {
            obj.SetValue(ClipPeriodProperty, value);
        }

        /// <summary>
        /// Property for only showing the containing part within the LimitdateTimePeriod
        /// </summary>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-02-27
        /// </remarks>
        public static readonly DependencyProperty ClipPeriodProperty =
            DependencyProperty.RegisterAttached("ClipPeriod", 
                                                typeof(bool), 
                                                typeof(DateTimePeriodPanel),
                                                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsParentArrange));

        #endregion

        #region LimitDateTimePeriodProperty
        /// <summary>
        /// LimitDateTimePeriodProperty, dependencyproperty
        /// Limits the childrens visuallayout. If not set to default, the item will only show within the period
        /// The Property is set on the object, not the panel!
        /// </summary>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-02-27
        /// </remarks>
        public static readonly DependencyProperty LimitDateTimePeriodProperty =
            DependencyProperty.RegisterAttached(
                "LimitDateTimePeriod",
                typeof(DateTimePeriod),
                typeof(DateTimePeriodPanel),
                new FrameworkPropertyMetadata(new DateTimePeriod(DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc), 
                    DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc)), FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsParentArrange));

        /// <summary>
        /// Gets the AttatchedDepenedencyProperty LimitDateTimePeriod
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        /// <remarks>
        /// See LimitdateTimePeriod
        /// Created by: henrika
        /// Created date: 2008-02-27
        /// </remarks>
        public static DateTimePeriod GetLimitDateTimePeriod(DependencyObject obj)
        {
            return (DateTimePeriod)obj.GetValue(LimitDateTimePeriodProperty);
        }

        /// <summary>
        /// Sets the AttatchedDepenedencyProperty LimitdateTimeperiodProperty
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="value">The value.</param>
        /// <remarks>
        /// See LimitdateTimePeriod
        /// Created by: henrika
        /// Created date: 2008-02-27
        /// </remarks>
        public static void SetLimitDateTimePeriod(DependencyObject obj, DateTimePeriod value)
        {
            obj.SetValue(LimitDateTimePeriodProperty, value);

        }
        #endregion

        #region DateTimePeriodProperty
        /// <summary>
        /// DateTimePeriodProperty, dependencyproperty
        /// Defines horizontallayout of VisualChildren
        /// </summary>
        /// Created by: henrika
        /// Created date: 2008-01-17
        /// </remarks>
        public static readonly DependencyProperty DateTimePeriodProperty =
            DependencyProperty.RegisterAttached(
                "DateTimePeriod", 
                typeof(DateTimePeriod), 
                typeof(DateTimePeriodPanel),
                new FrameworkPropertyMetadata(new DateTimePeriod(DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc), 
                    DateTime.SpecifyKind(DateTime.MinValue.AddMinutes(1), DateTimeKind.Utc)),
                    FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsParentArrange,RenderIfPanel));

        private static void RenderIfPanel(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = d as DateTimePeriodPanel;
            if (target!=null)
            {
                target.InvalidateArrange();
            }
        }

        public static DateTimePeriod GetDateTimePeriod(DependencyObject obj)
        {
            if ((bool)obj.GetValue(ClipPeriodProperty))
            {
                DateTimePeriod? limitDateTimePeriodProperty = (DateTimePeriod?)obj.GetValue(LimitDateTimePeriodProperty);
                DateTimePeriod? dateTimePeriodProperty = (DateTimePeriod?)obj.GetValue(DateTimePeriodProperty);

                DateTimePeriod? intersection =
                    (limitDateTimePeriodProperty.Value.Intersection(dateTimePeriodProperty.Value));

                if (intersection.HasValue)
                {
                    return intersection.Value;
                }
                return new DateTimePeriod(DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc),
                                          DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc).Add(
                                              TimeSpan.FromMinutes(1)));
            }

            return (DateTimePeriod)obj.GetValue(DateTimePeriodProperty);
        }

        /// <summary>
        /// Sets the Attatched property DateTimePeriodProperty, Updates layout
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetDateTimePeriod(DependencyObject obj, DateTimePeriod value)
        {
            obj.SetValue(DateTimePeriodProperty, value);
        }
        #endregion

        #region OverlapProperty
        /// <summary>
        /// DepenencyProperty  Overlap
        /// </summary>
        /// <remarks>
        /// Default is 0.0
        /// Affects render, supports two-way binding and animation
        /// Created by: henrika
        /// Created date: 2008-1-08
        /// </remarks>
        public static readonly DependencyProperty OverlapProperty =
            DependencyProperty.Register(
                "Overlap",
                typeof(double),
                typeof(DateTimePeriodPanel),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsArrange));
        /// <summary>
        /// Setters for Overlap property
        /// </summary>
        /// <param name="value">Value between 0-1 where 0 is no Overlap and 1 is items height.</param>
        /// <remarks>
        /// Is DependencyProperty
        /// Created by: henrika
        /// Created date: 2008-1-08
        /// </remarks>
        public double Overlap
        {
            get { return (double)GetValue(OverlapProperty); }
            set
            {
                if (value >= 0 && value <= 1)
                {
                    SetValue(OverlapProperty, value);
                }
            }

        }
        #endregion

        #region ItemHeightProperty
        //TODO: Set affects render
        public double MinimumItemHeight
        {
            get { return (double)GetValue(MinimumItemHeightProperty); }
            set { SetValue(MinimumItemHeightProperty, value); }
        }

        public static readonly DependencyProperty MinimumItemHeightProperty =
            DependencyProperty.Register("MinimumItemHeight", typeof(double), typeof(DateTimePeriodPanel), new UIPropertyMetadata(20d));

        public double MaximumItemHeight
        {
            get { return (double)GetValue(MaximumItemHeightProperty); }
            set { SetValue(MaximumItemHeightProperty, value); }
        }

        public static readonly DependencyProperty MaximumItemHeightProperty =
            DependencyProperty.Register("MaximumItemHeight", typeof(double), typeof(DateTimePeriodPanel), new UIPropertyMetadata(25d));
        #endregion

        #endregion

        #region methods

        /// <summary>
        /// Converts position to datetime
        /// </summary>
        /// <param name="arg">The x-position (format:1/96 inch wpf standard "twips")</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-02-18
        /// </remarks>
        public DateTime GetUtcDateTimeFromPosition(double arg)
        {
            return
                new LengthToTimeCalculator(GetDateTimePeriod(this), ActualWidth).
                    DateTimeFromPosition(arg);
        }

        public static TimeSpan GetTimeSpanFromHorizontalChange(FrameworkElement frameworkElement, double change, double interval)
        {
            DateTimePeriod dateTimePeriod = GetDateTimePeriod(frameworkElement);
            return CalculateSnapTime(frameworkElement.ActualWidth, dateTimePeriod, change, interval);
        }

        protected override Size MeasureElements(Size availableSize, UIElementCollection elements)
        {
            double panelMinutes = GetDateTimePeriod(this).ElapsedTime().TotalMinutes;

            if (Double.IsInfinity(availableSize.Width))
                availableSize = new Size(ActualWidth, availableSize.Height);

            if (Double.IsInfinity(availableSize.Height))
                availableSize = new Size(availableSize.Width, ActualHeight);

            elements.OfType<UIElement>().ForEach(
                c => c.Measure(new Size(GetElementWidth(panelMinutes, availableSize.Width, c),
                                        availableSize.Height)));

            //double height = Math.Min()
            Size retSize = new Size(Math.Max(0.0, availableSize.Width),
                                    Math.Max(MinimumItemHeight + 5,

                                             Math.Max(MinimumItemHeight * InternalChildren.Count * Overlap, Math.Min(availableSize.Height, InternalChildren.Count * Overlap * MaximumItemHeight)))); 
            return retSize;
        }

        protected override Size ArrangeElements(Size finalSize, UIElementCollection elements)
        {
            int numberOfElements = InternalChildren.Count;
            Size size = new Size(finalSize.Width,Math.Max(finalSize.Height,numberOfElements * Overlap * MaximumItemHeight));


            DateTimePeriod panelDateTimePeriod = GetDateTimePeriod(this);
            double panelMinutes = panelDateTimePeriod.ElapsedTime().TotalMinutes;
            double childHeight = size.Height / Math.Max(1,((numberOfElements * Overlap)));

            elements.OfType<UIElement>().All(c =>
            {
                DateTime startDateTime = GetDateTimePeriod(c).StartDateTime;

                //Check for negative values
                if (startDateTime < panelDateTimePeriod.StartDateTime)
                    startDateTime = panelDateTimePeriod.StartDateTime;

                double childPositionPlaceHolderWidth =
                    (startDateTime.Subtract(panelDateTimePeriod.StartDateTime).TotalMinutes / panelMinutes) *
                    size.Width;

                double childWidth = GetElementWidth(
                    panelMinutes,
                    size.Width,
                    c);

                c.Arrange(
                    new Rect(
                        childPositionPlaceHolderWidth,
                        InternalChildren.IndexOf(c) * Overlap * childHeight,
                        childWidth,
                        childHeight));

                return true;
            });

            Size retSize = new Size(size.Width,size.Height);
            return retSize;
        }

       

        #region private 
        /// <summary>
        /// Calculates the snap time.
        /// </summary>
        /// <param name="contentPresenterWidth">Width of the content presenter.</param>
        /// <param name="dateTimePeriod">The date time period.</param>
        /// <param name="arg">The arg.</param>
        /// <param name="interval">The interval.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-21
        /// </remarks>
        private static TimeSpan CalculateSnapTime(double contentPresenterWidth, DateTimePeriod dateTimePeriod, double arg, double interval)
        {
            TimeSpan t = TimeSpan.FromMilliseconds((dateTimePeriod.ElapsedTime().TotalMilliseconds / contentPresenterWidth) * arg);
            if (t.TotalMinutes < 1 && t.TotalMinutes > -1) return TimeSpan.FromTicks(0L);

            double minutesLeftOver = t.TotalMinutes % interval;
            double halfInterval = interval / 2;

            if (minutesLeftOver >= halfInterval)
                return t.Add(TimeSpan.FromMinutes(interval - minutesLeftOver));
            
            return t.Add(TimeSpan.FromMinutes(minutesLeftOver).Negate());
        }


        private double GetElementWidth(double panelMinutes, double panelWidth, UIElement element)
        {
            var panelPeriod = GetDateTimePeriod(this);           
            var elementPeriod = GetDateTimePeriod(element);

            if (IsDateOk(elementPeriod))
            {
                if (IsElementStartOutsidePanel(elementPeriod, panelPeriod))
                {
                    //This corrects the calculation of layers starting before the timeline starts
                    if (IsElementEndTimeLaterThanPanelStartTime(elementPeriod, panelPeriod))
                    {
						elementPeriod = new DateTimePeriod(panelPeriod.StartDateTime, elementPeriod.EndDateTime);                    	
                    }
                    else
                    {
                    	return 0;
                    }
                }
            }
            var childMinutes = elementPeriod.ElapsedTime().TotalMinutes;
            return (childMinutes / panelMinutes) * panelWidth;
        }

        private static bool IsElementEndTimeLaterThanPanelStartTime(DateTimePeriod elementPeriod, DateTimePeriod panelPeriod)
        {
            return elementPeriod.EndDateTime > panelPeriod.StartDateTime;
        }

        private static bool IsElementStartOutsidePanel(DateTimePeriod elementPeriod, DateTimePeriod panelPeriod)
        {
            return panelPeriod.StartDateTime > elementPeriod.StartDateTime;
        }

        private static bool IsDateOk(DateTimePeriod elementPeriod)
        {
            return elementPeriod.StartDateTime > DateTime.MinValue;
        }

        #endregion //private
        #endregion //methods
    }
}