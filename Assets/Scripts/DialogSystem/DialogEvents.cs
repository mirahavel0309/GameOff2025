using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public static class DialogEvents
{
    public static Action<List<DialogModel>, Image> OnDialogRequested;
}
