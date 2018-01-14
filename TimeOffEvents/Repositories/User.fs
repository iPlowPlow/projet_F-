namespace TimeOff.Repositories
open TimeOff

type UserRepository<'a> = {
    GetAll : unit -> 'a seq
    CreateTimeOff : TimeOffRequest -> 'a
    CancelTimeOff : TimeOffRequest -> 'a
    RequestCancelTimeOff : TimeOffRequest -> 'a
}