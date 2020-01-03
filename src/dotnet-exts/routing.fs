namespace DotNetExtensions


open DomainAgnostic
open Exts
open Microsoft.AspNetCore.Mvc.ActionConstraints
open System


module Routing =

    [<AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)>]
    type PortAttribute(value: int) =
        inherit Attribute()

        interface IActionConstraint with

            member __.Order = 0

            member __.Accept context =
                let port = context.RouteContext.HttpContext.Connection.LocalPort
                port = value


    [<AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)>]
    type HttpHeaderAttribute(name: string, value: string) =
        inherit Attribute()

        new(name: string) = HttpHeaderAttribute(name, null)


        interface IActionConstraint with

            member __.Order = 0

            member __.Accept context =
                let find =
                    context.RouteContext.HttpContext.Request.Headers
                    |> Map.OfKVP
                    |> Map.MapValues string
                    |> flip Map.tryFind

                match (find name, value) with
                | None, _ -> false
                | Some _, null -> true
                | Some v1, v2 -> v1 = v2
