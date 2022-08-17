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
            public const string ABOUT_WINDOW_NAME = "About";
            public const string ACCOUNT_WINDOW_NAME = "Account";
            public const string SPACE_WINDOW_NAME = "Space";
            public const string DIALOG_BUTTON_OK = "OK";
            public const string DIALOG_BUTTON_CANCEL = "Cancel";

            public const string CHECKING_TOKEN = "<color=#3D8DFF>Token id checking…</color>";
            public const string LOADING_SPACE_INFORMATION = "<color=#3D8DFF>Space resources loading...</color>";
            public const string ERROR = "<color=#ff0000>{0}</color>";
            public const string GET_DESIGNER_TOKEN = "Get a creator token id.";
            public const string INPUT_TOKEN_TIP = "Please input creator token id.";

            public const string PREVIEW_SPACE_DIALOG_TITLE = "Preview Space";

            public const string DELETE_SPACE_DIALOG_TITLE = "Delete Space";
            public const string DELETE_SPACE_MESSAGE = "Confirm to delete space? It cannot undo.";
            public const string CREATE_SPACE_ALREADY_EXISTS = "Object existed already.";
            public const string DIALOG_TITLE_ERROR = "Error";
            public const string CHECK_SPACE_ERROR = "Please create an object first.";

            public const string UPLOAD_SPACE_DIALOG_TITLE = "Upload Space";
            public const string UPLOAD_SPACE_PROGRESS = "Space uploading...";
            public const string UPLOAD_SPACE_MESSAGE = "Confirm to replace original resouces? It cannot undo.";
            public const string TEXTURE_SIZE_TO_LARGE = "Texture size can't exceed 2048*2048. The followings are oversized objects:";
            public const string HAD_CUSTOM_MONO = "Custom component is not allowed in the scene. The followings are inaccessible objects:";
            public const string TRIANGLES_COUNT_TO_LARGE = "The triangle amount can't exceed 1million in the scene. The current amount is:";
            public const string MAX_UPLOAD_S3_COUNT = "Already reach today's maximum uploads.";
        }
    }
}
