using Assets.Source.Enum;

namespace Assets.Source.UI.Model
{
    public readonly struct ToastModel
    {
        public readonly ToastType Type;
        public readonly string Message;

        public ToastModel(
            ToastType type,
            string message)
        {
            Type = type;
            Message = message;
        }
    }
}
