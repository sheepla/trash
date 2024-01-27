open System
open System.Reflection
open System.Runtime.InteropServices

// c.f.
//  https://learn.microsoft.com/en-us/windows/win32/shell/folder-movehere
module Flags =
    type MoveOption = int

    [<Literal>]
    let NotDisplayDialogBox: MoveOption = 4

    [<Literal>]
    let AutoRename: MoveOption = 8

    [<Literal>]
    let RespondYesToAll: MoveOption = 16

    [<Literal>]
    let PreserveUndo: MoveOption = 64

    [<Literal>]
    let ExpandWildCard: MoveOption = 128

    [<Literal>]
    let NotShowFileNameOnDialogBox: MoveOption = 256

    [<Literal>]
    let NotConfirmToCreateNewDirectory: MoveOption = 512

    [<Literal>]
    let NotCopySecurityAttributes: MoveOption = 1024

    [<Literal>]
    let NotOperateRecursively: MoveOption = 2048

    [<Literal>]
    let NotMoveConnectedFilesAsAGroup: MoveOption = 4096

type Explorer() =
    let shell = Activator.CreateInstance <| Type.GetTypeFromProgID "Shell.Application"

    let recyclebin =
        shell
            .GetType()
            .InvokeMember(
                name = "Namespace",
                invokeAttr = BindingFlags.InvokeMethod,
                binder = null,
                target = shell,
                args = [| box 0xA |]
            )

    member self.MoveToRecycleBin(path: string, options: Flags.MoveOption list) =
        shell
            .GetType()
            .InvokeMember(
                name = "MoveHere",
                invokeAttr = BindingFlags.InvokeMethod,
                binder = null,
                target = recyclebin,
                args = [| box path; box <| (options |> List.fold (+) 0) |]
            )
        |> ignore

    interface IDisposable with
        member self.Dispose() =
            Marshal.ReleaseComObject shell |> ignore
            // Not required?
            GC.SuppressFinalize self

[<EntryPoint>]
let main (argv: string array) : int =
    let fullPathes =
        argv |> Array.skip 1 |> Array.map (fun arg -> IO.Path.GetFullPath arg)

    let opts = [ Flags.ExpandWildCard ]

    try
        use explorer = new Explorer()

        fullPathes
        |> Array.iter (fun path ->
            printfn "Moving to $Recycle.bin: %s" path
            explorer.MoveToRecycleBin(path, opts))
    with ex ->
        eprintfn "Error: %s" ex.Message

    0
