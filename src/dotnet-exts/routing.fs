namespace DotNetExtensions


open DomainAgnostic
open Exts
open Microsoft.AspNetCore.Mvc.ActionConstraints
open Microsoft.Extensions.Primitives
open System
open System.Collections.Generic


module Routing =

    type HttpHeaderAttribute(name: string, value: string) =
        inherit Attribute()

        new(name: string) = HttpHeaderAttribute(name, null)


        interface IActionConstraint with

            member __.Order = 0

            member __.Accept context =
                let find =
                    context.RouteContext.HttpContext.Request.Headers
                    |> Seq.cast<KeyValuePair<string, StringValues>>
                    |> Seq.map (fun x -> x.Key.ToLower(), x.Value.ToString())
                    |> Map.ofSeq
                    |> flip Map.tryFind

                match (name.ToLower() |> find, value) with
                | None, _ -> false
                | Some _, null -> true
                | Some v1, v2 -> v1 = v2
