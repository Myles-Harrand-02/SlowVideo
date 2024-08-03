using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Diagnostics;

namespace GetStartedApp.Views;

public partial class MainWindow : Window
{
    private decimal? _spd;

    public decimal? Spd;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void ValChanged(object sender, NumericUpDownValueChangedEventArgs args)
    {
        Trace.WriteLine("It Changed!");
        Spd = NUP1.Value;
    }
}