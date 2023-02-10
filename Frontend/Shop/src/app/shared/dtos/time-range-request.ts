export class TimeRangeRequest {
  constructor(
    public appKey: string = '',
    public from: Date = new Date(Date.now()),
    public to: Date = new Date(Date.now())
  ){}
}
