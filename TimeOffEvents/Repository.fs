namespace TimeOff

type Repository<'a> = {
    GetAll : unit -> 'a seq
    CreateTimeOff : TimeOffRequest -> 'a
    ValidateTimeOff : TimeOffRequest -> 'a
    RefuseTimeOff : TimeOffRequest -> 'a
    CancelTimeOffByEmployee : TimeOffRequest -> 'a
    ValidateCancelTimeOff : TimeOffRequest -> 'a
    RefuseCancelTimeOff : TimeOffRequest -> 'a
}