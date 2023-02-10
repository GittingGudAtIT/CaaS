import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'caas-list-navigator',
  templateUrl: './list-navigator.component.html',
  styles: [
  ]
})
export class ListNavigatorComponent {
  @Input() maxPageIdx: number = 0;
  @Input() maxPagesAround: number = 1;
  @Input() pageIdx: number = 0;
  @Output() pageChangedEvent = new EventEmitter<number>();

  prefixes(): number[]{
    const res = [];
    for(var i = 0; i < Math.min(this.maxPagesAround, this.pageIdx - this.maxPagesAround); ++i)
      res.push(i);
    return res;
  }

  hotspot(): number[]{
    const res = [];
    for(var i = Math.max(0, this.pageIdx - this.maxPagesAround); i <= Math.min(this.pageIdx + this.maxPagesAround, this.maxPageIdx); ++i)
      res.push(i);
    return res;
  }

  postfixes(): number[]{
    const res = [];
    for(var i = Math.max(this.maxPageIdx + 1 - this.maxPagesAround, this.pageIdx + this.maxPagesAround + 1); i <= this.maxPageIdx; ++i)
      res.push(i);
    return res;
  }

  decIdx(){
    if(this.pageIdx > 0){
      this.pageIdx--;
      this.pageChangedEvent.emit(this.pageIdx);
    }
  }

  incIdx(){
    if(this.pageIdx < this.maxPageIdx){
      this.pageIdx++;
      this.pageChangedEvent.emit(this.pageIdx);
    }
  }

  setIdx(value: number){
    if(this.pageIdx !== value){
      this.pageIdx = value;
      this.pageChangedEvent.emit(this.pageIdx);
    }
  }
}
