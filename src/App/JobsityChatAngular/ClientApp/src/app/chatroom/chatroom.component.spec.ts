import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChatroomComponent } from './chatroom.component';

describe('ChatroomComponent', () => {
  let fixture: ComponentFixture<ChatroomComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ChatroomComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChatroomComponent);
    fixture.detectChanges();
  });

  it('should display a join group step', async(() => {
    const titleText = fixture.nativeElement.querySelector('strong').textContent;
    expect(titleText).toEqual('Create a group');
  }));

  it('should start with Bienvenido', async(() => {
    fixture.componentInstance.joined = true;
    fixture.detectChanges();
    const chatElement = fixture.nativeElement.querySelector('#chat');
    expect(chatElement.textContent).toContain('Bienvenido');
  }));
});
