namespace Eventarium.Client.Presentation;

public sealed class DisplayMotionProfile
{
    private const double ReferenceViewportWidth = 1_920;
    private const double MaximumViewportScale = 2;
    private const double MaximumEventWidth = 330;
    private const double EntryMarginRatio = 0.05;
    private static readonly double ReferenceTravelDistance = GetTravelDistance(ReferenceViewportWidth);
    private double _flightDurationScale = 1;

    public double FlightDurationScale => Volatile.Read(ref _flightDurationScale);

    public void SetViewportWidth(double viewportWidth)
    {
        if (!double.IsFinite(viewportWidth) || viewportWidth <= 0)
        {
            return;
        }

        double scale = GetTravelDistance(viewportWidth) / ReferenceTravelDistance;
        Volatile.Write(ref _flightDurationScale, Math.Clamp(scale, 1, MaximumViewportScale));
    }

    private static double GetTravelDistance(double viewportWidth) =>
        viewportWidth + (MaximumEventWidth * 2) + (viewportWidth * EntryMarginRatio);
}
