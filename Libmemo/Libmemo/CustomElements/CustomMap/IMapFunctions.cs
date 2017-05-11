using Xamarin.Forms.Maps;

namespace Libmemo {
    public interface IMapFunctions {
        void RaiseCameraPositionChange(Position position, float zoom);
        void RaiseInfoWindowClick(CustomPin pin);
        void RaiseUserLocationChange(Position position);
        void RaiseRouteInitializingSucceed();
        void RaiseRouteInitializingFailed();

        void SetRenderer(INativeMapFunction renderer);
    }
}
