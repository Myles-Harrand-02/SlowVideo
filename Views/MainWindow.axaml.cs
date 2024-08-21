using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using DynamicData;
using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace GetStartedApp.Views;

public partial class MainWindow : Window
{
    private int _spd;

    public int Spd;

    private List<string> _locations = new List<string>();

    private List<string> _names = new List<string>();

    private string _output;

    public MainWindow()
    {
        InitializeComponent();

        fileCounter.Text = "0" + " Files";

        fileSize.Text = "0.0 MB";
    }

    private void ValChanged(object sender, NumericUpDownValueChangedEventArgs args)
    {
        Trace.WriteLine("It Changed!");
        Spd = Convert.ToInt32(Math.Round((decimal)NUP1.Value, 0));
    }

    private async void btnFileOpen(object sender, RoutedEventArgs args)
    {
        // Get top level from the current control
        var topLevel = TopLevel.GetTopLevel(this);

        //This can also be applied for SaveFilePicker.
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Folder Containing Videos",
            AllowMultiple = true,
            FileTypeFilter = new[] { VideoAll }
        });

        fileCounter.Text = files.Count.ToString() + " Files";

        List<string> suffixes = new List<string>();
        ulong dataSize = 0;

        _locations = new List<string>();

        _names = new List<string>();

        foreach (var file in files)
        {

            var suff = file.Name.Substring(file.Name.Length - 4);

            if (suffixes.Contains(suff))
            {
                
            }
            else
            {
                suffixes.Add(suff);
            }

            var props = await file.GetBasicPropertiesAsync();

            if (props != null)
            {
                dataSize += props.Size.Value;
            }

            var location = file.Path;

            if (location != null)
            {
                _locations.Add( (location.ToString()).Substring(8) );

                _names.Add((file.Name).Remove(file.Name.Length - 4));
            }
        }

        fileTypes.Text = string.Join(", ", suffixes.ToArray());

        fileSize.Text = ((float)dataSize / 1e6).ToString("F1") + " MB";
    }

    private async void btnOutput(object sender, RoutedEventArgs args)
    {
        // Get top level from the current control
        var topLevel = TopLevel.GetTopLevel(this);

        //This can also be applied for SaveFilePicker.
        var files = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Folder Containing Videos",
            AllowMultiple = false,
        });

        if (files.Count > 0)
        {
            _output = files[0].Path.ToString().Substring(8);
            outputDir.Content = _output;
        }
        else
        {
            _output = string.Empty;
            outputDir.Content = "";
        }
    }

    private async void btnStart(object sender, RoutedEventArgs args)
    {
        if (_locations.Count > 0 & !string.IsNullOrEmpty(_output))
        {
            fileWarn.Text = "";
            fileTime.Text = "";

            fileTimeName.Text = "Completed:";
            int completed = 0;

            for (int i = 0; i < _locations.Count; i++)
            {
                var mediaInfo = await FFProbe.AnalyseAsync(_locations[i]);

                /*FFMpegArguments
                    .FromFileInput(_locations[i])
                    .OutputToFile(_output + "/" + _names[i] + ".h264", false, options => options
                        .WithVideoCodec(VideoCodec.LibX264)
                        .DisableChannel(Channel.Audio)
                        .WithFramerate(mediaInfo.PrimaryVideoStream.FrameRate / 2)
                        .WithDuration(mediaInfo.Duration.Multiply(2))
                        .ForceFormat("mp4")
                        .UsingMultithreading(true))
                    .ProcessSynchronously();*/

                string outputLoc = _output + "/" + _names[i] + ".mp4";

                string runCommand = string.Format("/C ffmpeg -i \"{0}\" -filter:v \"setpts={1}*PTS\" -an \"{2}\"", _locations[i], 2, outputLoc);

                // run ffmpeg command line argument
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = runCommand;

                process.StartInfo = startInfo;
                process.Start();

                completed++;
                fileTime.Text = completed.ToString();//mediaInfo.Duration.TotalSeconds.ToString() + " ";
            }
        }
        else if (_locations.Count == 0)
        {
            fileWarn.Text = "Please select a video file to convert!";
        }
        else
        {
            fileWarn.Text = "Please select an output file!";
        }

        fileTime.Text = "All Done!";
    }

    
    private static bool newSpeedMP4(string input, string output, double fps, TimeSpan duration)
    {
        return FFMpegArguments
                    .FromFileInput(input)
                    .OutputToFile(output, false, options => options
                        .WithVideoCodec(VideoCodec.LibX264)
                        .DisableChannel(Channel.Audio)
                        .WithFramerate(fps)
                        .WithDuration(duration)
                        .ForceFormat("mp4")
                        .UsingMultithreading(true))
                    .ProcessSynchronously();
    }
    

    public static FilePickerFileType VideoAll { get; } = new("All Videos")
    {
        Patterns = new[] { "*.mp4", "*.mov"},
        AppleUniformTypeIdentifiers = new[] { "public.video", "public.movie" },
        //MimeTypes = new[] { "image/*" }
    };
}