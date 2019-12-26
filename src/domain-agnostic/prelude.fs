namespace DomainAgnostic

open System
open System.Collections


[<AutoOpen>]
module Prelude =

    let CAST<'T> x =
        try
            x :> obj :?> 'T |> Some
        with _ -> None

    let echo thing =
        match CAST<String>(thing :> obj) with
        | Some s -> Console.WriteLine s
        | None -> printfn "%+A" thing

    let trace thing =
        echo thing
        thing

    let ToString thing = thing.ToString()

    let flip f x y = f y x

    let constantly x _ = x

    let tuple x y = (x, y)

    let (++) = Seq.append

    let ENV() =
        Environment.GetEnvironmentVariables()
        |> Seq.cast<DictionaryEntry>
        |> Seq.map (fun d -> d.Key :?> string, d.Value :?> string)
        |> Map.ofSeq


type Container<'T>(value: 'T) =
    member __.Boxed = value
