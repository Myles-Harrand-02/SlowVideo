using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Diagnostics;

namespace GetStartedApp.Views;

public partial class MainWindow : Window
{
    private int _spd;

    public int Spd;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void ValChanged(object sender, NumericUpDownValueChangedEventArgs args)
    {
        Trace.WriteLine("It Changed!");
        Spd = Convert.ToInt32(Math.Round((decimal)NUP1.Value, 0));
    }
}