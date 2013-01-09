using System.Runtime.InteropServices;

namespace OpenWrap.VisualStudio.SolutionAddIn
{
    [Guid(ComConstants.ADD_IN_GUID_2012)]
    [ProgId(ComConstants.ADD_IN_PROGID_2012)]
    [ComVisible(true)]
    public class OpenWrapVisualStudioAddIn2012 : OpenWrapVisualStudioAddIn
    {
        public OpenWrapVisualStudioAddIn2012() : base("11.0", ComConstants.ADD_IN_PROGID_2012) { }
    }
}