using Assets.Source.Enum;

namespace Assets.UI.Models
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
