namespace DomainAgnostic.Reflection

open DomainAgnostic
open FSharp.Reflection
open FSharp.Quotations
open FSharp.Quotations.DerivedPatterns
open FSharp.Quotations.Patterns


module Record =

    let asMap (record: 'T) =
        FSharpType.GetRecordFields(typeof<'T>)
        |> Seq.ofArray
        |> Seq.map (fun p -> p.Name, p.GetValue(record))
        |> Map.ofSeq

    let ofMap<'T> map =
        let typ = typeof<'T>

        let idx =
            FSharpType.GetRecordFields(typ)
            |> Seq.ofArray
            |> Seq.map (fun f -> f.Name)
            |> flip Seq.findIndex

        try
            let members =
                map
                |> Map.toSeq
                |> Seq.sortBy
                    (fst
                     >> (=)
                     >> idx)
                |> Seq.map snd
                |> Seq.toArray
            FSharpValue.MakeRecord(typ, members) :?> 'T |> Some
        with _ -> None


module Enum =

    let isCase (c: Expr<_ -> 'T>) =
        match c with
        | Lambdas(_, NewUnionCase(uci, _)) ->
            let tagReader = FSharpValue.PreComputeUnionTagReader(uci.DeclaringType)
            fun (v: 'T) -> (tagReader v) = uci.Tag
        | _ -> failwith "Invalid expression"
        | _ -> failwith "Invalid expression"
