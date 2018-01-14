namespace TimeOff.Repository
open TimeOff

type UserRepository<'a> = {
    GetById : int -> 'a seq option
    GetCurrentBalanceById : int -> SoldeJour option
    GetCurrentUserById : int -> Person option
    CreateTimeOff : TimeOffRequest -> 'a
    CancelTimeOffByEmployee : TimeOffRequest -> 'a
}