using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI.Model
{
    /// <summary>
    /// click mouse right to make obstacle
    /// </summary>
    /// <param name="Item"></param>
    public record CellMouseRightClickMessage(PathItem Item);
}
