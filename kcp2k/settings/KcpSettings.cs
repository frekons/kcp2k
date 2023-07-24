using kcp2k;
using System.Buffers;

public static class KcpSettings
{
    public const bool DualMode = false; // if true => listens IPV6 and IPV4, if false => listens only IPV4
    public const bool NoDelay = true;
    public const bool CongestionWindow = false;
    public const int FastResend = 2;
    public const int Interval = 10;
    public const int SendWindowSize = 8192;
    public const int ReceiveWindowSize = 8192;
    public const int Timeout = 60 * 1000; // 60 seconds -> 1 minute
    public const int LongTimeout = 600 * 1000; // 600 seconds -> 10 minutes
    public const int MaxRetransmits = 20; // Kcp.DEADLINK
    public const bool MaximizeSendReceiveBuffersToOSLimit = true;

    public static KcpConfig GetConfig()
    {
        var config = new KcpConfig(DualMode: KcpSettings.DualMode, NoDelay: KcpSettings.NoDelay, Interval: KcpSettings.Interval, FastResend: KcpSettings.FastResend, CongestionWindow: KcpSettings.CongestionWindow, SendWindowSize: KcpSettings.SendWindowSize, ReceiveWindowSize: KcpSettings.ReceiveWindowSize, Timeout: KcpSettings.Timeout, MaxRetransmits: KcpSettings.MaxRetransmits);

        return config;
    }

    //

    public static ArrayPool<byte> KcpBufferPool = ArrayPool<byte>.Create();

    public static void WarmPool()
    {
        var kcpPool = KcpSettings.KcpBufferPool;

        for (int i = 0; i < 2048; ++i)
        {
            var rentedBuffer = kcpPool.Rent(4);

            kcpPool.Return(rentedBuffer, clearArray: true);
        }

        for (int i = 0; i < 1024; ++i)
        {
            var rentedBuffer = kcpPool.Rent(1024);

            kcpPool.Return(rentedBuffer, clearArray: true);
        }

        for (int i = 0; i < 1024; ++i)
        {
            var rentedBuffer = kcpPool.Rent(2048);

            kcpPool.Return(rentedBuffer, clearArray: true);
        }

        for (int i = 0; i < 1024; ++i)
        {
            var rentedBuffer = kcpPool.Rent(4096);

            kcpPool.Return(rentedBuffer, clearArray: true);
        }

        for (int i = 0; i < 2048; ++i)
        {
            var rentedBuffer = kcpPool.Rent(1 + KcpConnection.ReliableMaxMessageSize());

            kcpPool.Return(rentedBuffer, clearArray: true);
        }
    }
}