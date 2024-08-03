namespace GetStartedApp.ViewModels;

using Avalonia.Controls;
using ReactiveUI;
using System.Diagnostics;
using System.Windows.Input;
//using ReactiveUI.Fody.Helpers;

public class MainWindowViewModel : ViewModelBase
{
    //variables for checking checkbox
    private bool _currentCheck;
    private bool _previousCheck;

    private int _spdMaxFPS;

    private int _spdMaxPer;

    public int SpdMaxFPS
    {
        get => _spdMaxFPS;
        set => _spdMaxFPS = 240;
    }

    public int SpdMaxPer
    {
        get => _spdMaxPer;
        set => _spdMaxPer = 4;
    }

    private int _spd;
    public int Spd
    {
        get => _spd;
        set
        {
            if (value < 0)
            {
                value = 0;
            }
            else if (value > _spdMaxFPS)
            { 
                value = _spdMaxFPS;
            }
            else
            {
                this.RaiseAndSetIfChanged(ref _spd, value);
            }
        }
        //set => this.RaiseAndSetIfChanged(ref _spd, value);
    }

    /*[Reactive]
    public int Spd { get; set; }*/

    public ICommand TwoThirdSpd { get; }
    public ICommand HalfSpd { get; }
    public ICommand ThirdSpd { get; }
    public ICommand QuarterSpd { get; }

    public MainWindowViewModel()
    {
        TwoThirdSpd = ReactiveCommand.Create(() => Spd = (Spd * 2) / 3);
        HalfSpd = ReactiveCommand.Create(() => Spd = Spd / 2);
        ThirdSpd = ReactiveCommand.Create(() => Spd = Spd  / 3);
        QuarterSpd = ReactiveCommand.Create(() => Spd = Spd / 4);
    }
}
