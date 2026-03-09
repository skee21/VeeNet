using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace VeeNet.Services;

public sealed class AudioCaptureService : IDisposable
{
    private MMDevice? _device;
    private WasapiLoopbackCapture? _capture;
    private bool _disposed;

    public float CurrentPeak
    {
        get
        {
            try
            {
                return _device?.AudioMeterInformation?.MasterPeakValue ?? 0f;
            }
            catch
            {
                return 0f;
            }
        }
    }

    public void Start()
    {
        var enumerator = new MMDeviceEnumerator();
        _device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);

        _capture = new WasapiLoopbackCapture(_device);
        _capture.DataAvailable += (_, _) => { };
        _capture.RecordingStopped += (_, _) => { };
        _capture.StartRecording();
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
