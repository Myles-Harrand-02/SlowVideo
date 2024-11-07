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
    private int _spd; //video speed (unused)

    public int Spd;

    private List<string> _locations = new List<string>(); //stores file locations of input files

    private List<string> _names = new List<string>(); //stores main name of input files

    private string _output; //stores folder location for the output folder

    public MainWindow()
    {
        InitializeComponent();

        fileCounter.Text = "0" + " Files"; //initialize information to display 0 files selected

        fileSize.Text = "0.0 MB";
    }

    private void ValChanged(object sender, NumericUpDownValueChangedEventArgs args)
    {
        //.WriteLine("It Changed!");
        Spd = Convert.ToInt32(Math.Round((decimal)NUP1.Value, 0));
    }

    private async void btnFileOpen(object sender, RoutedEventArgs args)
    {
        // Get top level from the current control
        var topLevel = TopLevel.GetTopLevel(this);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            //input file selector, allowing multiple video files
            Title = "Select Folder Containing Videos",
            AllowMultiple = true,
            FileTypeFilter = new[] { VideoAll }
        });

        fileCounter.Text = files.Count.ToString() + " Files";

        //finds all the video file types selected and displays them to the user
        List<string> suffixes = new List<string>();
        ulong dataSize = 0;

        _locations = new List<string>();

        _names = new List<string>();

        foreach (var file in files)
        {
            //collect file types from suffix
            var suff = file.Name.Substring(file.Name.Length - 4);

            //only add to suffixes if the current suffix has not been used
            if (suffixes.Contains(suff))
            {
                
            }
            else
            {
                suffixes.Add(suff);
            }

            //fetch file information to calculate size of selected videos
            var props = await file.GetBasicPropertiesAsync();

            if (props != null)
            {
                dataSize += props.Size.Value;
            }

            var location = file.Path;

            if (location != null)
            {
                //adds the full location of each selected file
                _locations.Add( (location.ToString()).Substring(8) );

                //adds the main name (without suffix) of each selected file
                _names.Add((file.Name).Remove(file.Name.Length - 4));
            }
        }

        //displays the suffixes to the user with commas separating each
        fileTypes.Text = string.Join(", ", suffixes.ToArray());

        //displays the file size in megabytes and rounds the final value
        fileSize.Text = ((float)dataSize / 1e6).ToString("F1") + " MB";
    }

    private async void btnOutput(object sender, RoutedEventArgs args)
    {

        var topLevel = TopLevel.GetTopLevel(this);

        var files = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            //output folder selector, allowing a single folder
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

                //old code using FFMpeg wrapper
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

                //new code running FFMpeg program stored locally. I use this method for speed
                string runCommand = string.Format("/C ffmpeg -i \"{0}\" -threads 8 -r 24 -filter:v \"setpts={1}*PTS\" -an \"{2}\"", _locations[i], 2.5, outputLoc);

                // run ffmpeg command line argument
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = runCommand;

                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();

                completed++;
                fileTime.Text = completed.ToString();//mediaInfo.Duration.TotalSeconds.ToString() + " ";
            }
        }

        //checks the user has selected a valid input file and output folder
        else if (_locations.Count == 0)
        {
            fileWarn.Text = "Please select a video file to convert!";
        }
        else
        {
            fileWarn.Text = "Please select an output file!";
        }

        fileTime.Text = "All Done!"; //the task has finished
    }

    //old function for using FFMpeg wrapper to change video framerate
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