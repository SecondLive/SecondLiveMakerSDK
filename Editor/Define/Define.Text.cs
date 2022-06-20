using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecondLive.Maker.Editor
{
    public static partial class Define
    {
        public static class Text
        {
            public const string ACCOUNT_WINDOW_NAME = "Account";
            public const string CHECKING_TOKEN = "<color=#0000ff>Token id checking…</color>";
            public const string LOADING_SPACE_INFORMATION = "<color=#0000ff>Space information loading…</color>";
            public const string ERROR = "<color=#ff0000>{0}</color>";
            public const string DELETE_SPACE_DIALOG_TITLE = "Delete Space";
            public const string DELETE_SPACE_MESSAGE = "Confirm to delete space? It cannot undo.";
            public const string DIALOG_BUTTON_OK = "OK";
            public const string DIALOG_BUTTON_CANCEL = "Cancel";
            public const string UPLOAD_SPACE_MESSAGE = "Confirm to replace original resouces? It cannot undo.";
            public const string CREATE_SPACE_ALREADY_EXISTS = "Object existed already.";
            public const string DIALOG_TITLE_ERROR = "Error";
            public const string CHECK_SPACE_ERROR = "Please create an object first.";
            public const string GET_DESIGNER_TOKEN = "Get a creator token id.";
            public const string INPUT_TOKEN_TIP = "Please input creator token id.";
            public const string MAX_UPLOAD_S3_COUNT = "Already reach today's maximum uploads.";
        }
    }
}
