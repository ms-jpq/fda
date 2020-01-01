namespace DomainAgnostic

open System
open System.Text


module Encode =

    let base64encode (str: string) =
        str
        |> Encoding.UTF8.GetBytes
        |> Convert.ToBase64String

    let base64decode str =
        str
        |> Convert.FromBase64String
        |> Encoding.UTF8.GetString
