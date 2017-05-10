using Xamarin.Forms.Maps;

namespace Libmemo {
    public interface INativeMapFunction {
        void SetLinearRoute(Position from, Position to);
        void SetCalculatedRoute(Position from, Position to);
        void DeleteRoute();
    }
}
