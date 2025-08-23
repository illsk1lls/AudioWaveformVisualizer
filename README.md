<h1 align="center">AudioWaveformVisualizer</h1>

<p align="center"><img src="https://github.com/illsk1lls/AudioWaveformVisualizer/blob/main/.readme/visualizer.png?raw=true"></p>
<div align="center">

Testing/Build Instructions (Solution file is included in project folder):

* Download Repo
* Extract Zip
* Open Visual Studio
* Select "Open a Project or Solution"
* Select [ExtractedFolder]\AudioWaveformVisualizer\AudioWaveformVisualizer.sln
* Click the Green Arrow ;)

I am using this (similar) code elsewhere wanted to share it for others to use/learn from.

No external dependencies are required

Windows Audio Session API (WASAPI) is used in loopback mode to capture system audio playback from the default render device, averaging multi-channel samples and rendering them in waveform

Output changes (e.g. switching speakers or headphones) are tracked using IMMNotificationClient event triggers, restarting capture as needed to maintain visualization