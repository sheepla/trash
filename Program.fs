open System
open System.Reflection
open System.Runtime.InteropServices

// c.f.
//  https://learn.microsoft.com/en-us/windows/win32/shell/folder-movehere
module Flags =
    type MoveOption = int

    [<Literal>]
    let NotDisplayDialogBox: MoveOption = 2 <<< 1

    [<Literal>]
    let AutoRename: MoveOption = 2 <<< 3

    [<Literal>]
    let RespondYesToAll: MoveOption = 2 <<< 4

    [<Literal>]
    let PreserveUndo: MoveOption = 2 <<< 4

    [<Literal>]
    let ExpandWildCard: MoveOption = 2 <<< 5

    [<Literal>]
    let NotShowFileNameOnDialogBox: MoveOption = 2 <<< 6

    [<Literal>]
    let NotConfirmToCreateNewDirectory: MoveOption = 2 <<< 7

    [<Literal>]
    let NotCopySecurityAttributes: MoveOption = 2 <<< 8

    [<Literal>]
    let NotOperateRecursively: MoveOption = 2 <<< 9

    [<Literal>]
    let NotMoveConnectedFilesAsAGroup: MoveOption = 2 <<< 10

type Explorer () =
    let shell =
        Activator.CreateInstance <| Type.GetTypeFromProgID "Shell.Application"

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
                "Namespace",
                BindingFlags.InvokeMethod,
                null,
                recyclebin,
                [| box path; box <| (options |> List.fold (+) 0) |]
            )
        |> ignore

    interface IDisposable with
        member self.Dispose() =
            Marshal.ReleaseComObject <| shell |> ignore
            // Not required?
            GC.SuppressFinalize self


[<EntryPoint>]
let main (argv: string array) : int =
    let fullPathes =
        argv |> Array.skip 1 |> Array.map (fun arg -> IO.Path.GetFullPath arg)

    let opts = [ Flags.NotConfirmToCreateNewDirectory; Flags.ExpandWildCard ]

    use explorer = new Explorer()

    fullPathes
    |> Array.iter (fun path ->
        try
            explorer.MoveToRecycleBin(path, opts)
        with ex ->
            eprintfn "%s" ex.Message
    )

    0
