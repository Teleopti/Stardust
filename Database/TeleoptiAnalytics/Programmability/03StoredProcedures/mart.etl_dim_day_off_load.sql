/ * * * * * *   O b j e c t :     S t o r e d P r o c e d u r e   [ m a r t ] . [ e t l _ d i m _ d a y _ o f f _ l o a d ]         S c r i p t   D a t e :   1 0 / 0 3 / 2 0 0 8   1 7 : 1 8 : 2 3   * * * * * * /  
 I F     E X I S T S   ( S E L E C T   *   F R O M   s y s . o b j e c t s   W H E R E   o b j e c t _ i d   =   O B J E C T _ I D ( N ' [ m a r t ] . [ e t l _ d i m _ d a y _ o f f _ l o a d ] ' )   A N D   t y p e   i n   ( N ' P ' ,   N ' P C ' ) )  
 D R O P   P R O C E D U R E   [ m a r t ] . [ e t l _ d i m _ d a y _ o f f _ l o a d ]  
 G O  
  
 - -   = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =  
 - -   A u t h o r : 	 	 K J  
 - -   C r e a t e   d a t e :   2 0 0 8 - 1 0 - 0 3  
 - -   D e s c r i p t i o n : 	 L o a d s   d a y   o f f   f r o m   s t g _ d a y _ o f f   t o   d i m _ d a y _ o f f .  
 - -   U p d a t e   d a t e   l o g  
 - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -  
 - -   W h e n 	 	 	 W h o   W h a t  
 - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -  
 - -   2 0 0 9 - 0 2 - 1 1 	 K J 	 N e w   m a r t   s c h e m a    
 - -   2 0 0 8 - 1 0 - 0 3 	 	 D a y   O f f   n o t   a v a i l a b l e   y e t .   L o a d s   o n l y   - 1   N o t   D e f i n e d   u n t i l   t h e n .  
 - -   2 0 0 8 - 1 0 - 2 4 	 K J 	 L o a d   o n e   D a y   O f f   p e r   B U .  
 - -   2 0 1 0 - 0 1 - 1 4 	 D J 	 A d d i n g   D a y O f f   b u t   w i t h   D a y _ O f f _ N a m e   a s   P K   ( N o t   D a y _ O f f _ c o d e   a s   o r i g i n a l   D W   d e s i g n e d )  
 - -   2 0 1 1 - 0 2 - 0 8 	 D J 	 # 1 3 4 7 1   D a y s   o f f   w i t h   s a m e   n a m e   o n   t w o   B U   d o e s   n o t   w o r k   i n   a n a l y t i c s  
 - -   = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =  
 C R E A T E   P R O C E D U R E   [ m a r t ] . [ e t l _ d i m _ d a y _ o f f _ l o a d ]  
 @ b u s i n e s s _ u n i t _ c o d e   u n i q u e i d e n t i f i e r 	 	  
 A S  
 - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -  
 - -   N o t   D e f i n e d  
 S E T   I D E N T I T Y _ I N S E R T   m a r t . d i m _ d a y _ o f f   O N  
  
 I N S E R T   I N T O   m a r t . d i m _ d a y _ o f f  
 	 (  
 	 d a y _ o f f _ i d ,  
 	 d a y _ o f f _ n a m e ,    
 	 d i s p l a y _ c o l o r ,  
 	 b u s i n e s s _ u n i t _ i d ,  
 	 d a t a s o u r c e _ i d  
 	 )  
 S E L E C T    
 	 d a y _ o f f _ i d 	 	 	 = - 1 ,    
 	 d a y _ o f f _ n a m e 	 	 = ' N o t   D e f i n e d ' ,    
 	 d i s p l a y _ c o l o r 	 	 =   - 1 ,  
 	 b u s i n e s s _ u n i t _ i d 	 = - 1 ,  
 	 d a t a s o u r c e _ i d 	 	 =   - 1  
 W H E R E   N O T   E X I S T S   ( S E L E C T   *   F R O M   m a r t . d i m _ d a y _ o f f   w h e r e   d a y _ o f f _ i d   =   - 1 )  
  
 S E T   I D E N T I T Y _ I N S E R T   m a r t . d i m _ d a y _ o f f   O F F  
 - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -  
 - -   u p d a t e   c h a n g e s  
 U P D A T E   m a r t . d i m _ d a y _ o f f  
 S E T    
 	 d a y _ o f f _ c o d e 	 =   s . d a y _ o f f _ c o d e ,  
 	 d i s p l a y _ c o l o r 	 =   s . d i s p l a y _ c o l o r  
 F R O M  
 	 [ s t a g e ] . [ s t g _ d a y _ o f f ]   s  
 I N N E R   J O I N   [ m a r t ] . [ d i m _ b u s i n e s s _ u n i t ]   b u  
 O N   s . b u s i n e s s _ u n i t _ c o d e   =   b u . b u s i n e s s _ u n i t _ c o d e  
 W H E R E    
 	 s . d a y _ o f f _ n a m e   =   m a r t . d i m _ d a y _ o f f . d a y _ o f f _ n a m e  
 A N D  
 	 b u . b u s i n e s s _ u n i t _ i d   =   m a r t . d i m _ d a y _ o f f . b u s i n e s s _ u n i t _ i d  
 	  
 - -   I n s e r t   n e w    
 I N S E R T   I N T O   m a r t . d i m _ d a y _ o f f  
 	 (  
 	 d a y _ o f f _ c o d e ,    
 	 d a y _ o f f _ n a m e ,    
 	 d i s p l a y _ c o l o r ,  
 	 b u s i n e s s _ u n i t _ i d ,  
 	 d a t a s o u r c e _ i d  
 	 )  
 S E L E C T    
 	 d a y _ o f f _ c o d e 	 	 =   s . d a y _ o f f _ c o d e ,  
 	 d a y _ o f f _ n a m e 	 	 =   s . d a y _ o f f _ n a m e ,   - - T h i s   i s   p a r t   o f   t h e   P K    
 	 d i s p l a y _ c o l o r 	 	 =   s . d i s p l a y _ c o l o r ,  
 	 b u s i n e s s _ u n i t _ i d 	 =   b u . b u s i n e s s _ u n i t _ i d ,   - - T h i s   i s   p a r t   o f   t h e   P K  
 	 d a t a s o u r c e _ i d 	 	 =   1  
 F R O M  
 	 [ s t a g e ] . [ s t g _ d a y _ o f f ]   s 	  
 I N N E R   J O I N  
 	 [ m a r t ] . [ d i m _ b u s i n e s s _ u n i t ]   b u  
 O N  
 	 s . b u s i n e s s _ u n i t _ c o d e   = b u . b u s i n e s s _ u n i t _ c o d e 	  
 W H E R E    
 	 N O T   E X I S T S   ( S E L E C T   d a y _ o f f _ i d  
 	 	 	 	 F R O M   m a r t . d i m _ d a y _ o f f   d  
 	 	 	 	 W H E R E   d . d a y _ o f f _ n a m e   =   s . d a y _ o f f _ n a m e  
 	 	 	 	 A N D   d . b u s i n e s s _ u n i t _ i d   =   b u . b u s i n e s s _ u n i t _ i d  
 	 	 	 	 A N D   d . d a t a s o u r c e _ i d = 1 )  
 - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -  
  
 G O 