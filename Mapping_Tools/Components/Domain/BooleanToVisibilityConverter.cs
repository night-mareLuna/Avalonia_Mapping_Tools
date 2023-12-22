using Mapping_Tools.Classes.BeatmapHelper.Enums;

namespace Mapping_Tools.Components.Domain
{
    class BooleanToVisibilityConverter : BooleanConverter<Visibility> {
        public BooleanToVisibilityConverter() :
            base(Visibility.Visible, Visibility.Collapsed)
        { }
    }
}
