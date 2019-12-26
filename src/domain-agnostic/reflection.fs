namespace DomainAgnostic

open FSharp.Reflection
open FSharp.Quotations
open FSharp.Quotations.DerivedPatterns
open FSharp.Quotations.Patterns


module Reflection =

    let asMap (record: 'T) =
        FSharpType.GetRecordFields(typeof<'T>)
        |> Seq.ofArray
        |> Seq.map (fun p -> p.Name, p.GetValue(record))
        |> Map.ofSeq

    let isCase (c: Expr<_ -> 'T>) =
        match c with
        | Lambdas(_, NewUnionCase(uci, _)) ->
            let tagReader = FSharpValue.PreComputeUnionTagReader(uci.DeclaringType)
            fun (v: 'T) -> (tagReader v) = uci.Tag
        | _ -> failwith "Invalid expression"
