using Avalonia.Media;

namespace Mapping_Tools.Components.ObjectVisualiser {
    public class HitObjectElementMarker(double progress, double size, Brush brush)
    {
        public double Progress { get; set; } = progress;
        public double Size { get; set; } = size;
        public Brush Brush { get; set; } = brush;
    }
}