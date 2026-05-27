# LinuxDemo — running .NET MAUI on Linux

This sample demonstrates the **"Linux head project"** pattern for getting a
.NET MAUI app running on Linux desktops using the experimanetal **GTK4 backend**
(now hosted in [`dotnet/maui-labs`](https://github.com/dotnet/maui-labs/tree/main/platforms/Linux.Gtk4)).

Developed for [Talk .NET Meetup - May 2026](https://www.meetup.com/microsoft-events-in-prague/events/314890386/)

```
samples/
├── LinuxDemo/           ← Shared MAUI project (Android, iOS, Mac Catalyst, Windows, + plain net10.0)
│   ├── App.xaml         ← Shared XAML, ViewModels, services
│   ├── MainPage.xaml    ← Widget gallery: buttons, sliders, switch, stepper,
│   └── ...                progress bar, date/time pickers, CollectionView, CarouselView, FilePicker
└── LinuxDemo.Linux/     ← Linux-specific head (net10.0)
    ├── Program.cs       ← GtkMauiApplication entry point
    └── MauiProgram.cs   ← builder.UseMauiAppLinuxGtk4<App>()
```

There is no official `net10.0-linux` TFM, so we add a plain `net10.0` target to
the shared MAUI project. The Linux head simply references that and bootstraps
GTK4 — Linux becomes "just another head" alongside Android, iOS, etc.

## "Can I build the Linux app on Windows / macOS?"

**Yes — compile, but not run.**

| Action | Windows | macOS | Linux |
|---|---|---|---|
| `dotnet build LinuxDemo.Linux`                                       | ✅ produces `LinuxDemo.Linux.dll` | ✅ | ✅ |
| `dotnet publish LinuxDemo.Linux -r linux-x64 --self-contained`        | ✅ produces a Linux folder ready to scp | ✅ | ✅ |
| `dotnet run --project LinuxDemo.Linux`                                | ❌ app starts but fails to load GTK4 / `libgtk-4` | ❌ | ✅ |
| CI: build once on any OS, ship the `linux-x64` publish output         | ✅ | ✅ | ✅ |

Why? The output is a managed DLL plus a tiny native bootstrapper; the actual
window rendering uses `libgtk-4`, `libadwaita`, `libwebkitgtk-6.0` — Linux-only
shared libraries that only exist on a Linux box.

## Prerequisites (on the Linux box)

The GTK4 backend P/Invokes into the system's GTK4, GLib, libadwaita, and
WebKitGTK shared libraries — they must be installed at runtime. The `-dev`
packages are only required if you want headers (e.g. for native interop or
to build extra C bindings); installing them pulls in the runtime libs as
dependencies, which is the easiest way to get a green build.

### Debian / Ubuntu (22.04+, Pop!_OS, Mint, WSL)

```bash
sudo apt update
sudo apt install -y \
    libgtk-4-dev \
    libadwaita-1-dev \
    libwebkitgtk-6.0-dev \
    libgirepository1.0-dev \
    libglib2.0-dev \
    libcairo2-dev \
    libpango1.0-dev \
    gsettings-desktop-schemas
```

> On Ubuntu 22.04 `libwebkitgtk-6.0-dev` lives in `universe`:
> `sudo add-apt-repository universe && sudo apt update` first.
> Ubuntu 24.04 / Debian trixie have it in `main` already.

Runtime-only (deployment image, no dev headers):

```bash
sudo apt install -y \
    libgtk-4-1 libadwaita-1-0 libwebkitgtk-6.0-4 \
    libgirepository-1.0-1 gsettings-desktop-schemas
```

### Fedora 40+ / RHEL 9+

```bash
sudo dnf install -y \
    gtk4-devel libadwaita-devel webkitgtk6.0-devel \
    gobject-introspection-devel glib2-devel cairo-devel pango-devel
```

### Arch / Manjaro

```bash
sudo pacman -S --needed gtk4 libadwaita webkitgtk-6.0 \
    gobject-introspection glib2 cairo pango
```

### openSUSE Tumbleweed

```bash
sudo zypper install gtk4-devel libadwaita-devel webkitgtk-6_0-devel \
    gobject-introspection-devel glib2-devel cairo-devel pango-devel
```

### Verify the libs are reachable

```bash
pkg-config --modversion gtk4            # expect 4.10+
pkg-config --modversion libadwaita-1    # expect 1.4+
pkg-config --modversion webkitgtk-6.0   # expect 2.44+ (only if you use WebView)
ldconfig -p | grep -E 'libgtk-4|libadwaita|libwebkitgtk-6'
```

If any of those come up empty, the app will throw `DllNotFoundException` on
startup pointing at the missing native library.

### .NET SDK + workloads

```bash
# .NET 10 GA SDK (one-shot install script)
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 10.0 --install-dir $HOME/.dotnet
export PATH="$HOME/.dotnet:$PATH"

# MAUI workload
dotnet workload install maui
```

You can confirm with `dotnet --info` — you should see
`10.0.1xx` and the `maui` workload listed.

## Build & run

### On Linux (full loop)

```bash
cd samples/LinuxDemo.Linux
dotnet run
```

### From Windows or macOS → Linux artifact

```bash
# Cross-compile a self-contained Linux build:
cd samples/LinuxDemo.Linux
dotnet publish -c Release -r linux-x64 --self-contained -o ./publish

# scp publish/  user@linuxbox:~/linuxdemo/
# ssh user@linuxbox 'cd linuxdemo && ./LinuxDemo.Linux'
```

### The other heads (just for completeness — they're not Linux)

```bash
dotnet build ../LinuxDemo -f net10.0-android
dotnet build ../LinuxDemo -f net10.0-windows10.0.19041.0   # Windows only
dotnet build ../LinuxDemo -f net10.0-maccatalyst           # macOS only
```

## What the demo shows

`MainPage.xaml` is a single-page **widget gallery** that exercises common MAUI
controls so you can see GTK4 native rendering side-by-side with the same
controls on Android / Windows / iOS:

- Buttons, click counter
- Slider with emoji feedback
- Three sliders → live RGB color preview
- Stepper + ProgressBar + ActivityIndicator (fake "dotnet build" runner)
- DatePicker + TimePicker
- Entry + auto-sizing Editor
- CollectionView with a list of Linux distros
- CarouselView with IndicatorView dots

## Notes

- `Platform.Maui.Linux.Gtk4` is **experimental**. Pin to a specific version (look
  at NuGet for the latest preview) if you want reproducible builds.
- The `Tmds.DBus.Protocol` transitive dep raises `NU1903`; the GTK4 backend
  uses it for portals. Either upgrade transitively or ignore until upstream
  rolls a new release.
- If you change the MAUI project's namespace or `RootNamespace`, update the
  `using LinuxDemo;` line in `LinuxDemo.Linux/MauiProgram.cs`.
