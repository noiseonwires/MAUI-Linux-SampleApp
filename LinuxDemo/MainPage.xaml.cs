using System.Runtime.InteropServices;

namespace LinuxDemo;

public partial class MainPage : ContentPage
{
    int count = 0;

    public MainPage()
    {
        InitializeComponent();

        PlatformLabel.Text = $"Platform: {DeviceInfo.Platform} ({DeviceInfo.Idiom})";
        RuntimeLabel.Text = $"Runtime: {RuntimeInformation.FrameworkDescription} on {RuntimeInformation.OSDescription}";
        DisplayLabel.Text = $"Display: {DeviceDisplay.MainDisplayInfo.Width}x{DeviceDisplay.MainDisplayInfo.Height} @ {DeviceDisplay.MainDisplayInfo.Density:F1}";

        // CollectionView data
        DistrosView.ItemsSource = new[]
        {
            new Distro("🐧", "Debian",       "The universal OS"),
            new Distro("🟠", "Ubuntu",       "Linux for human beings"),
            new Distro("🎩", "Fedora",       "Bleeding-edge stable"),
            new Distro("🏔",  "Alpine",       "Tiny, secure, container-friendly"),
            new Distro("🌀", "Arch",         "Btw, I use Arch"),
            new Distro("🦎", "openSUSE",     "Get the chameleon"),
            new Distro("🛡",  "Pop!_OS",     "Built for hackers and makers"),
        };

        // CarouselView data
        TipsCarousel.ItemsSource = new[]
        {
            "💡 The shared MAUI project adds a plain net11.0 TFM so the Linux head can reference it as a library.",
            "🔧 `builder.UseMauiAppLinuxGtk4<App>()` wires GTK4 into the same MauiAppBuilder pipeline you already know.",
            "🚀 Same XAML, same C#, same Essentials APIs — only the *head* project differs per platform.",
            "📦 `Platform.Maui.Linux.Gtk4` is community-maintained in dotnet/maui-labs. Production-ready it is not. Fun it is.",
        };
    }

    // -------- Button + counter --------
    private void OnCounterClicked(object? sender, EventArgs e)
    {
        count++;
        CounterBtn.Text = count == 1 ? "Clicked 1 time" : $"Clicked {count} times";
    }

    // -------- Mood slider --------
    private void OnMoodChanged(object? sender, ValueChangedEventArgs e)
    {
        var face = e.NewValue switch
        {
            < 15 => "😩",
            < 35 => "😟",
            < 55 => "😐",
            < 75 => "🙂",
            < 95 => "😄",
            _    => "🎉",
        };
        MoodLabel.Text = $"How's the build going? {face}  ({e.NewValue:F0}%)";
    }

    // -------- RGB color mixer --------
    private void OnColorChanged(object? sender, ValueChangedEventArgs e)
    {
        if (ColorPreview is null) return;
        var r = (byte)RedSlider.Value;
        var g = (byte)GreenSlider.Value;
        var b = (byte)BlueSlider.Value;
        ColorPreview.BackgroundColor = Color.FromRgb(r, g, b);
        HexLabel.Text = $"#{r:X2}{g:X2}{b:X2}";
        // Pick a readable text colour.
        var luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255.0;
        HexLabel.TextColor = luminance > 0.55 ? Colors.Black : Colors.White;
    }

    // -------- Stepper + progress + spinner --------
    private void OnJobsChanged(object? sender, ValueChangedEventArgs e)
        => StepperLabel.Text = $"Parallel jobs: {(int)e.NewValue}";

    private async void OnRunBuildClicked(object? sender, EventArgs e)
    {
        RunBuildBtn.IsEnabled = false;
        BuildSpinner.IsRunning = true;
        BuildProgress.Progress = 0;

        var steps = 25;
        var delayMs = Math.Max(20, 200 / (int)JobsStepper.Value); // more jobs = faster
        for (int i = 1; i <= steps; i++)
        {
            await Task.Delay(delayMs);
            BuildProgress.Progress = i / (double)steps;
        }

        BuildSpinner.IsRunning = false;
        RunBuildBtn.IsEnabled = true;
        RunBuildBtn.Text = "dotnet build ✓";
    }

    // -------- Date / Time pickers --------
    private void OnScheduleChanged(object? sender, DateChangedEventArgs e) => UpdateSchedule();
    private void OnScheduleTimeChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TimePicker.Time)) UpdateSchedule();
    }
    private void UpdateSchedule()
        => ScheduleLabel.Text = $"Next build: {ScheduleDate.Date:yyyy-MM-dd} at {ScheduleTime.Time:hh\\:mm}";

    // -------- Entry --------
    private void OnNameChanged(object? sender, TextChangedEventArgs e)
        => GreetingLabel.Text = string.IsNullOrWhiteSpace(e.NewTextValue)
            ? string.Empty
            : $"👋 Hello, {e.NewTextValue}!";

    // -------- CollectionView selection --------
    private void OnDistroSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Distro d)
            DistroLabel.Text = $"You picked {d.Emoji} {d.Name} — {d.Tagline}";
    }

    private sealed record Distro(string Emoji, string Name, string Tagline);
}
