namespace DotNetExtensions


open DomainAgnostic
open Exts
open Microsoft.AspNetCore.Mvc.ActionConstraints
open Microsoft.Extensions.Primitives
open System
open System.Collections.Generic


module Routing =

    type HttpHeaderAttribute(name: string, value: string option) =
        inherit Attribute()
        interface IActionConstraint with

            member __.Order = 0

            member __.Accept context =
                let find =
                    context.RouteContext.HttpContext.Request.Headers
                    |> Seq.cast<KeyValuePair<string, StringValues>>
                    |> Map.OfKVP
                    |> flip Map.tryFind

                match (find name, value) with
                | None, _ -> false
                | Some v1, Some v2 -> ToString v1 = v2
                | Some _, _ -> true
