namespace DomainAgnostic

open FSharp.Reflection
open FSharp.Quotations
open FSharp.Quotations.DerivedPatterns
open FSharp.Quotations.Patterns
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

    let asMap (record: 'T) =
        [ for p in FSharpType.GetRecordFields(typeof<'T>) -> p.Name, p.GetValue(record) ]
        |> Map.ofSeq

    let (++) = Seq.append

    let (?) a b = Option.defaultValue b a

    let isCase (c: Expr<_ -> 'T>) =
        match c with
        | Lambdas(_, NewUnionCase(uci, _)) ->
            let tagReader = FSharpValue.PreComputeUnionTagReader(uci.DeclaringType)
            fun (v: 'T) -> (tagReader v) = uci.Tag
        | _ -> failwith "Invalid expression"

    let ENV() =
        Environment.GetEnvironmentVariables()
        |> Seq.cast<DictionaryEntry>
        |> Seq.map (fun d -> d.Key :?> string, d.Value :?> string)
        |> Map.ofSeq


type Container<'T>(value: 'T) =
    member __.Boxed = value
