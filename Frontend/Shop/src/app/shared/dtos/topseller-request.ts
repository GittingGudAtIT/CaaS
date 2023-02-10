export class TopsellerRequest {
  constructor(
    public appkey: string = '',
    public from: Date = new Date(Date.now()),
    public to: Date = new Date(Date.now()),
    public count: number = 0
  ){}
}
