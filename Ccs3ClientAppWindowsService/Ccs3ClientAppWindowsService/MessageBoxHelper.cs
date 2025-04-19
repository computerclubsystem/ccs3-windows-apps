
using System;
using System.Runtime.InteropServices;

namespace Ccs3ClientAppWindowsService;

// Sample usage
// MessageBoxHelper.Show("Test message", "Test caption", MessageBoxButtons.Ok, MessageBoxDefaultButton.Button1, MessageBoxModal.System);
 
public class MessageBoxHelper {

    [DllImport("user32.dll")]
    private static extern int MessageBoxA(IntPtr hWnd, string lpText, string lpCaption, uint uType);

    public static MessageBoxResult Show(string text) => (MessageBoxResult)MessageBoxA(IntPtr.Zero, text, "\0", (uint)MessageBoxButtons.Ok);

    public static MessageBoxResult Show(string text, string caption) => (MessageBoxResult)MessageBoxA(IntPtr.Zero, text, caption, (uint)MessageBoxButtons.Ok);

    public static MessageBoxResult Show(string text, string caption, MessageBoxButtons buttons) => (MessageBoxResult)MessageBoxA(IntPtr.Zero, text, caption, (uint)buttons);

    public static MessageBoxResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon) => (MessageBoxResult)MessageBoxA(IntPtr.Zero, text, caption, ((uint)buttons) | ((uint)icon));

    public static MessageBoxResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton button) => (MessageBoxResult)MessageBoxA(IntPtr.Zero, text, caption, ((uint)buttons) | ((uint)icon) | ((uint)button));

    public static MessageBoxResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton button, MessageBoxModal modal) => (MessageBoxResult)MessageBoxA(IntPtr.Zero, text, caption, ((uint)buttons) | ((uint)icon) | ((uint)button) | ((uint)modal));

    public static MessageBoxResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxDefaultButton button) => (MessageBoxResult)MessageBoxA(IntPtr.Zero, text, caption, ((uint)buttons) | ((uint)button));

    public static MessageBoxResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxDefaultButton button, MessageBoxModal modal) => (MessageBoxResult)MessageBoxA(IntPtr.Zero, text, caption, ((uint)buttons) | ((uint)button) | ((uint)modal));

    public static MessageBoxResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxModal modal) => (MessageBoxResult)MessageBoxA(IntPtr.Zero, text, caption, ((uint)buttons) | ((uint)icon) | ((uint)modal));

    public static MessageBoxResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxModal modal) => (MessageBoxResult)MessageBoxA(IntPtr.Zero, text, caption, ((uint)buttons) | ((uint)modal));
}

public enum MessageBoxButtons {
    /// <summary>
    /// The message box contains one push button: OK. This is the default.
    /// </summary>
    Ok = 0x00000000,

    /// <summary>
    /// The message box contains two push buttons: OK and Cancel.
    /// </summary>
    OkCancel = 0x00000001,

    /// <summary>
    /// The message box contains three push buttons: Abort, Retry, and Ignore.
    /// </summary>
    AbortRetryIgnore = 0x00000002,

    /// <summary>
    /// The message box contains three push buttons: Yes, No, and Cancel.
    /// </summary>
    YesNoCancel = 0x00000003,

    /// <summary>
    /// The message box contains two push buttons: Yes and No.
    /// </summary>
    YesNo = 0x00000004,

    /// <summary>
    /// The message box contains two push buttons: Retry and Cancel.
    /// </summary>
    RetryCancel = 0x00000005,

    /// <summary>
    /// The message box contains three push buttons: Cancel, Try Again, Continue.
    /// </summary>
    CancelTryIgnore = 0x00000006,

    /// <summary>
    /// Adds a Help button to the message box.
    /// </summary>
    Help = 0x00004000,
}

/// <summary>
/// The message box returns an integer value that indicates which button the user clicked.
/// </summary>
public enum MessageBoxResult {
    /// <summary>
    /// The 'OK' button was selected.
    /// </summary>
    Ok = 1,

    /// <summary>
    /// The 'Cancel' button was selected.
    /// </summary>
    Cancel,

    /// <summary>
    /// The 'Abort' button was selected.
    /// </summary>
    Abort,

    /// <summary>
    /// The 'Retry' button was selected.
    /// </summary>
    Retry,

    /// <summary>
    /// The 'Ignore' button was selected.
    /// </summary>
    Ignore,

    /// <summary>
    /// The 'Yes' button was selected.
    /// </summary>
    Yes,

    /// <summary>
    /// The 'No' button was selected.
    /// </summary>
    No,

    /// <summary>
    /// The 'TryAgain' button was selected.
    /// </summary>
    TryAgain = 10,

    /// <summary>
    /// The 'Continue' button was selected.
    /// </summary>
    Continue
}

/// <summary>
/// To indicate the default button, specify one of the following values.
/// </summary>
public enum MessageBoxDefaultButton : uint {
    /// <summary>
    /// The first button is the default button.
    /// </summary>
    Button1 = 0x00000000,

    /// <summary>
    /// The second button is the default button.
    /// </summary>
    Button2 = 0x00000100,

    /// <summary>
    /// The third button is the default button.
    /// </summary>
    Button3 = 0x00000200,

    /// <summary>
    /// The fourth button is the default button.
    /// </summary>
    Button4 = 0x00000300,
}

/// <summary>
/// To indicate the modality of the dialog box, specify one of the following values.
/// </summary>
public enum MessageBoxModal : uint {
    /// <summary>
    /// The user must respond to the message box before continuing work in the window identified by the hWnd parameter. However, the user can move to the windows of other threads and work in those windows. Depending on the hierarchy of windows in the application, the user may be able to move to other windows within the thread. All child windows of the parent of the message box are automatically disabled, but pop-up windows are not.
    /// </summary>
    Application = 0x00000000,

    /// <summary>
    /// Same as <see cref="Application"/> except that the message box has the Top Most style. Use system-modal message boxes to notify the user of serious, potentially damaging errors that require immediate attention (for example, running out of memory).
    /// </summary>
    System = 0x00001000,

    /// <summary>
    /// Same as <see cref="Application"/> except that all the top-level windows belonging to the current thread are disabled if the hWnd parameter is NULL. Use this flag when the calling application or library does not have a window handle available but still needs to prevent input to other windows in the calling thread without suspending other threads.
    /// </summary>
    Task = 0x00002000
}

/// <summary>
/// To display an icon in the message box, specify one of the following values.
/// </summary>
public enum MessageBoxIcon : uint {
    /// <summary>
    /// A stop-sign icon appears in the message box.
    /// </summary>
    Error = 0x00000010,

    /// <summary>
    /// A question-mark icon appears in the message box.
    /// </summary>
    /// <remarks>
    /// The question-mark message icon is no longer recommended because it does not clearly represent a specific type of message and because the phrasing of a message as a question could apply to any message type. In addition, users can confuse the message symbol question mark with Help information. Therefore, do not use this question mark message symbol in your message boxes. The system continues to support its inclusion only for backward compatibility.
    /// </remarks>
    Question = 0x00000020,

    /// <summary>
    /// An exclamation-point icon appears in the message box.
    /// </summary>
    Warning = 0x00000030,

    /// <summary>
    /// An icon consisting of a lowercase letter `i` in a circle appears in the message box.
    /// </summary>
    Information = 0x00000040
}
