using System.Threading.Tasks;
using Xamarin.Forms.Maps;

namespace Libmemo {
    public interface INativeMapFunction {
        Task SetLinearRouteAsync(Position from, Position to);
        Task SetCalculatedRouteAsync(Position from, Position to);

        void DeleteRoute();
    }
}
