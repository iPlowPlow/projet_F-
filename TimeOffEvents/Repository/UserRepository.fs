namespace TimeOff.Repository
open TimeOff

type UserRepository<'a> = {
    GetById : int -> 'a seq option
    GetCurrentBalanceById : int -> SoldeJour option
    CreateTimeOff : TimeOffRequest -> 'a
    CancelTimeOffByEmployee : TimeOffRequest -> 'a
}