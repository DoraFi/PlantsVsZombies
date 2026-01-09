using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PlantsVsZombies.Models;

namespace PlantsVsZombies.VisualControls;

public partial class LocationCarousel : UserControl
{
    public static readonly DependencyProperty LocationsProperty =
        DependencyProperty.Register(nameof(Locations), typeof(List<LocationType>), typeof(LocationCarousel),
            new PropertyMetadata(new List<LocationType>(), OnLocationsChanged));

    public static readonly DependencyProperty SelectedLocationProperty =
        DependencyProperty.Register(nameof(SelectedLocation), typeof(LocationType), typeof(LocationCarousel),
            new FrameworkPropertyMetadata(LocationType.GrassLawn, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedLocationChanged));

    public List<LocationType> Locations
    {
        get => (List<LocationType>)GetValue(LocationsProperty);
        set => SetValue(LocationsProperty, value);
    }

    public LocationType SelectedLocation
    {
        get => (LocationType)GetValue(SelectedLocationProperty);
        set => SetValue(SelectedLocationProperty, value);
    }

    public LocationCarousel()
    {
        InitializeComponent();
        Loaded += LocationCarousel_Loaded;
    }

    private void LocationCarousel_Loaded(object sender, RoutedEventArgs e)
    {
        // Ensure selected location is valid
        if (Locations != null && Locations.Count > 0)
        {
            if (!Locations.Contains(SelectedLocation))
            {
                SelectedLocation = Locations[0];
            }
        }
        UpdateNavigationButtons();
        ScrollToSelectedLocation();
    }

    private static void OnLocationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LocationCarousel carousel)
        {
            carousel.UpdateNavigationButtons();
            carousel.ScrollToSelectedLocation();
        }
    }

    private static void OnSelectedLocationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LocationCarousel carousel)
        {
            carousel.UpdateNavigationButtons();
            carousel.ScrollToSelectedLocation();
        }
    }

    private void LeftButton_Click(object sender, RoutedEventArgs e)
    {
        NavigateLeft();
    }

    private void RightButton_Click(object sender, RoutedEventArgs e)
    {
        NavigateRight();
    }

    private void LocationItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is LocationType location)
        {
            SelectedLocation = location;
            ScrollToSelectedLocation();
        }
    }

    private void Indicator_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is LocationType location)
        {
            SelectedLocation = location;
            ScrollToSelectedLocation();
        }
    }

    private void NavigateLeft()
    {
        var currentIndex = Locations.IndexOf(SelectedLocation);
        if (currentIndex > 0)
        {
            SelectedLocation = Locations[currentIndex - 1];
        }
    }

    private void NavigateRight()
    {
        var currentIndex = Locations.IndexOf(SelectedLocation);
        if (currentIndex < Locations.Count - 1)
        {
            SelectedLocation = Locations[currentIndex + 1];
        }
    }

    private void ScrollToSelectedLocation()
    {
        if (LocationItemsControl == null || LocationScrollViewer == null)
            return;

        var selectedIndex = Locations.IndexOf(SelectedLocation);
        if (selectedIndex < 0)
            return;

        // Calculate the position to scroll to
        var itemWidth = 400.0; // 380 width + 20 margin (10 on each side)
        var scrollPosition = selectedIndex * itemWidth - (LocationScrollViewer.ViewportWidth / 2) + (itemWidth / 2);
        
        LocationScrollViewer.ScrollToHorizontalOffset(Math.Max(0, scrollPosition));
    }

    private void UpdateNavigationButtons()
    {
        if (LeftButton == null || RightButton == null || Locations == null)
            return;

        var currentIndex = Locations.IndexOf(SelectedLocation);
        LeftButton.IsEnabled = currentIndex > 0;
        RightButton.IsEnabled = currentIndex < Locations.Count - 1;
    }

    private void LocationScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        // Optional: Update indicators based on scroll position for smooth scrolling experience
    }
}
