using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace VeeNet.Services;

public sealed class AudioCaptureService : IDisposable
{
    private MMDevice? _device;
    private WasapiLoopbackCapture? _capture;
    private bool _disposed;

    public float CurrentPeak { get; private set; }

    public void Start()
    {
        var enumerator = new MMDeviceEnumerator();
        _device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);

        _capture = new WasapiLoopbackCapture(_device);
        _capture.DataAvailable += OnDataAvailable;
        _capture.RecordingStopped += (_, _) => { };
        _capture.StartRecording();
    }

    private void OnDataAvailable(object? sender, NAudio.Wave.WaveInEventArgs e)
    {
        if (_device != null)
        {
            CurrentPeak = _device.AudioMeterInformation.MasterPeakValue;
        }
    }

    public void Stop()
    {
        _capture?.StopRecording();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _capture?.StopRecording();
        _capture?.Dispose();
        _capture = null;

        _device?.Dispose();
        _device = null;
    }
}
