using System;

namespace Microsoft.MixedReality.Toolkit.UI
{
    [Serializable]
    public class DialogButtonContext
    {
        public string Label;
        public DialogButtonType ButtonType;

        public DialogButtonContext(string label, DialogButtonType type)
        {
            Label = label;
            ButtonType = type;
        }
    }
}
