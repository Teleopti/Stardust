I F     E X I S T S   ( S E L E C T   *   F R O M   s y s . o b j e c t s   W H E R E   o b j e c t _ i d   =   O B J E C T _ I D ( N ' [ m a r t ] . [ e t l _ f a c t _ s c h e d u l e _ p r e f e r e n c e _ l o a d ] ' )   A N D   t y p e   i n   ( N ' P ' ,   N ' P C ' ) )  
 D R O P   P R O C E D U R E   [ m a r t ] . [ e t l _ f a c t _ s c h e d u l e _ p r e f e r e n c e _ l o a d ]  
 G O  
  
 - -   = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =  
 - -   A u t h o r : 	 	 K J  
 - -   C r e a t e   d a t e :   2 0 0 8 - 1 1 - 1 9  
 - -   D e s c r i p t i o n : 	 W r i t e   s c h e d u l e   p r e f e r e n c e s   f r o m   s t a g i n g   t a b l e   ' s t g _ s c h e d u l e _ p r e f e r e n c e '  
 - - 	 	 	 	 t o   d a t a   m a r t   t a b l e   ' f a c t _ s c h e d u l e _ p r e f e r e n c e ' .  
 - -   U p d a t e s : 	 	 2 0 0 9 - 0 1 - 1 6  
 - - 	 	 	 	 2 0 0 9 - 0 2 - 0 9   S t a g e   m o v e d   t o   m a r t   d b ,   r e m o v e d   v i e w   K J  
 - - 	 	 	 	 2 0 0 9 - 0 1 - 1 6   C h a n g e d   f i e l d s   i n   s t g   t a b l e   K J  
 - - 	 	 	 	 2 0 0 8 - 1 2 - 0 1   C h a n g e d   D e l e t e   s t a t e m e n t   f o r   m u l t i   B U .   K J  
 - - 	 	 	 	 2 0 0 9 - 1 2 - 0 9   S o m e   i n t e r m e d i a t e   h a r d c o d e d   s t u f f   o n   d a y _ o f f _ i d ,   H e n r y   G r e i j e r   a n d   J o n a s   N o r d h .  
 - - 	 	 	 	 2 0 1 0 - 1 0 - 1 2   # 1 2 0 5 5   -   E T L   -   c a n t   l o a d   p r e f e r e n c e s  
 - - 	 	 	 	 2 0 1 1 - 0 9 - 2 7   F i x   s t a r t / e n d   t i m e s   =   0  
 - -   I n t e r f a c e : 	 s m a l l d a t e t i m e ,   w i t h   o n l y   d a t e p a r t !   N o   t i m e   a l l o w e d  
 - -   = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =  
 - - e x e c   m a r t . e t l _ f a c t _ s c h e d u l e _ p r e f e r e n c e _ l o a d   ' 2 0 0 9 - 0 2 - 0 1 ' , ' 2 0 0 9 - 0 2 - 1 7 '  
  
 C R E A T E   P R O C E D U R E   [ m a r t ] . [ e t l _ f a c t _ s c h e d u l e _ p r e f e r e n c e _ l o a d ]    
 @ s t a r t _ d a t e   s m a l l d a t e t i m e ,  
 @ e n d _ d a t e   s m a l l d a t e t i m e ,  
 @ b u s i n e s s _ u n i t _ c o d e   u n i q u e i d e n t i f i e r  
 	  
 A S  
  
 - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -  
 - -   < T e m p o r a r y >  
 - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -  
 - - 1 )   L o a d   s t a g e . s t g _ d a y _ o f f   f r o m   t h i s   p r o c e d u r e  
 - - 2 )   t h e n   l o a d   m a r t . d i m _ d a y _ o f f   f r o m   t h i s   p r o c e d u r e  
 - -   W e   e x p e c t   t a b l e :   s t a g e . s t g _ s c h e d u l e _ p r e f e r e n c e  
 - -   t o   b e   f i l l e d   b y   t h e   E T L   p r o c e s s  
 - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -  
 - - t r u n c a t e   s t a g e   d i m  
 T R U N C A T E   T A B L E   [ s t a g e ] . [ s t g _ d a y _ o f f ]  
  
 - -   G e t   s t a g e   f a c t   i n t o   s t a g e   d i m e n s i o n  
 I N S E R T   I N T O   [ s t a g e ] . [ s t g _ d a y _ o f f ]  
 	 (  
 	 [ d a y _ o f f _ n a m e ]  
 	 , [ b u s i n e s s _ u n i t _ c o d e ]  
 	 , [ d a y _ o f f _ c o d e ]  
 	 , [ d i s p l a y _ c o l o r ]  
 	 , [ d a t a s o u r c e _ i d ]  
 	 , [ i n s e r t _ d a t e ]  
 	 , [ u p d a t e _ d a t e ]  
 	 , [ d a t a s o u r c e _ u p d a t e _ d a t e ]  
 	 )  
  
 S E L E C T  
 	 [ d a y _ o f f _ n a m e ] 	 	 	 =   s . d a y _ o f f _ n a m e ,  
 	 [ b u s i n e s s _ u n i t _ c o d e ] 	 =   s . b u s i n e s s _ u n i t _ c o d e ,  
 	 [ d a y _ o f f _ c o d e ] 	 	 	 =   N U L L ,  
 	 [ d i s p l a y _ c o l o r ] 	 	 	 =   ' - 1 2 5 8 2 7 8 4 ' ,  
         [ d a t a s o u r c e _ i d ] 	 	 	 =   m i n ( s . d a t a s o u r c e _ i d ) ,  
 	 [ i n s e r t _ d a t e ] 	 	 	 =   G E T D A T E ( ) ,  
 	 [ u p d a t e _ d a t e ] 	 	 	 =   G E T D A T E ( ) ,  
 	 [ d a t a s o u r c e _ u p d a t e _ d a t e ] =   G E T D A T E ( )  
 F R O M   s t a g e . s t g _ s c h e d u l e _ p r e f e r e n c e   s  
 W H E R E   d a y _ o f f _ n a m e   I S   N O T   N U L L  
 G R O U P   B Y   s . d a y _ o f f _ n a m e , s . b u s i n e s s _ u n i t _ c o d e  
  
 - - L o a d   m a r t   d i m e n s i o n  
 E X E C   [ m a r t ] . [ e t l _ d i m _ d a y _ o f f _ l o a d ]  
 - -   < / T e m p o r a r y >  
 - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -  
 D E C L A R E   @ s t a r t _ d a t e _ i d 	 I N T  
 D E C L A R E   @ e n d _ d a t e _ i d 	 I N T  
  
 - - D e c l a r e  
 D E C L A R E   @ m a x _ s t a r t _ d a t e   s m a l l d a t e t i m e  
 D E C L A R E   @ m i n _ s t a r t _ d a t e   s m a l l d a t e t i m e  
  
 - - i n i t  
 S E L E C T      
 	 @ m a x _ s t a r t _ d a t e =   m a x ( r e s t r i c t i o n _ d a t e ) ,  
 	 @ m i n _ s t a r t _ d a t e =   m i n ( r e s t r i c t i o n _ d a t e )  
 F R O M  
 	 S t a g e . s t g _ s c h e d u l e _ p r e f e r e n c e  
 - - s e l e c t   *   f r o m   v _ s t g _ s c h e d u l e _ p r e f e r e n c e  
 - - R e s e t   @ s t a r t _ d a t e ,   @ e n d _ d a t e   t o    
 S E T 	 @ s t a r t _ d a t e   =   C A S E   W H E N   @ m i n _ s t a r t _ d a t e   >   @ s t a r t _ d a t e   T H E N   @ m i n _ s t a r t _ d a t e   E L S E   @ s t a r t _ d a t e   E N D  
 S E T 	 @ e n d _ d a t e 	 =   C A S E   W H E N   @ m a x _ s t a r t _ d a t e   <   @ e n d _ d a t e   T H E N   @ m a x _ s t a r t _ d a t e   E L S E   @ e n d _ d a t e 	 E N D  
  
 - - T h e r e   m u s t   n o t   b e   a n y   t i m e v a l u e   o n   t h e   i n t e r f a c e   v a l u e s ,   s i n c e   t h a t   w i l l   m e s s   t h i n g s   u p   a r o u n d   m i d n i g h t !  
 - - C o n s i d e r :   D E C L A R E   @ e n d _ d a t e   s m a l l d a t e t i m e ; S E T   @ e n d _ d a t e   =   ' 2 0 0 6 - 0 1 - 3 1   2 3 : 5 9 : 3 0 ' ; S E L E C T   @ e n d _ d a t e  
 S E T   @ s t a r t _ d a t e   =   C O N V E R T ( s m a l l d a t e t i m e , C O N V E R T ( n v a r c h a r ( 3 0 ) ,   @ s t a r t _ d a t e ,   1 1 2 ) )   - - I S O   y y y y m m d d  
 S E T   @ e n d _ d a t e 	 =   C O N V E R T ( s m a l l d a t e t i m e , C O N V E R T ( n v a r c h a r ( 3 0 ) ,   @ e n d _ d a t e ,   1 1 2 ) )  
  
 - - N o t   c u r r e n t l y   n e e d e d   s i n c e   w e   n o w   d e l e t e   o n   s h i f t _ s t a r t t i m e   ( i n s t e a d   o f   _ i d )  
 S E T   @ s t a r t _ d a t e _ i d   = 	 ( S E L E C T   d a t e _ i d   F R O M   d i m _ d a t e   W H E R E   @ s t a r t _ d a t e   =   d a t e _ d a t e )  
 S E T   @ e n d _ d a t e _ i d 	   = 	 ( S E L E C T   d a t e _ i d   F R O M   d i m _ d a t e   W H E R E   @ e n d _ d a t e   =   d a t e _ d a t e )  
  
  
 S E T   @ b u s i n e s s _ u n i t _ i d   =   ( S E L E C T   b u s i n e s s _ u n i t _ i d   F R O M   m a r t . d i m _ b u s i n e s s _ u n i t   W H E R E   b u s i n e s s _ u n i t _ c o d e   =   @ b u s i n e s s _ u n i t _ c o d e )  
  
 D E L E T E   F R O M   m a r t . f a c t _ s c h e d u l e _ p r e f e r e n c e  
 W H E R E   d a t e _ i d   b e t w e e n   @ s t a r t _ d a t e _ i d   A N D   @ e n d _ d a t e _ i d  
 A N D   b u s i n e s s _ u n i t _ i d   =   @ b u s i n e s s _ u n i t _ i d  
  
  
 / *  
 D E L E T E   F R O M   m a r t . f a c t _ s c h e d u l e _ p r e f e r e n c e  
 W H E R E   d a t e _ i d   b e t w e e n   @ s t a r t _ d a t e _ i d   A N D   @ e n d _ d a t e _ i d  
 	 A N D   b u s i n e s s _ u n i t _ i d   =    
 	 (  
 	 	 S E L E C T   D I S T I N C T  
 	 	 	 b u . b u s i n e s s _ u n i t _ i d  
 	 	 F R O M    
 	 	 	 S t a g e . s t g _ s c h e d u l e _ p r e f e r e n c e   s  
 	 	 I N N E R   J O I N  
 	 	 	 m a r t . d i m _ b u s i n e s s _ u n i t   b u  
 	 	 O N  
 	 	 	 s . b u s i n e s s _ u n i t _ c o d e   =   b u . b u s i n e s s _ u n i t _ c o d e  
 	 )  
 * /  
 - - D E L E T E   F R O M   m a r t . f a c t _ s c h e d u l e _ p r e f e r e n c e   W H E R E   d a t e _ i d   b e t w e e n   @ s t a r t _ d a t e _ i d   A N D   @ e n d _ d a t e _ i d  
  
 - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -  
 - -   I n s e r t   r o w s  
  
 I N S E R T   I N T O   m a r t . f a c t _ s c h e d u l e _ p r e f e r e n c e  
 	 (  
 	 d a t e _ i d ,    
 	 i n t e r v a l _ i d ,    
 	 p e r s o n _ i d ,    
 	 s c e n a r i o _ i d ,    
 	 p r e f e r e n c e _ t y p e _ i d ,    
 	 s h i f t _ c a t e g o r y _ i d ,    
 	 d a y _ o f f _ i d ,    
 	 p r e f e r e n c e s _ r e q u e s t e d _ c o u n t ,    
 	 p r e f e r e n c e s _ a c c e p t e d _ c o u n t ,    
 	 p r e f e r e n c e s _ d e c l i n e d _ c o u n t ,    
 	 b u s i n e s s _ u n i t _ i d ,    
 	 d a t a s o u r c e _ i d ,    
 	 d a t a s o u r c e _ u p d a t e _ d a t e  
 	 )  
 	  
 - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -  
 - -   < Q u i c k   f i x   f o r   # 1 2 0 5 5 >  
 - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -  
 - - D u p l i c a t e   p r e f s   e x i s t   o n   s o m e   a g e n t s .   D i f f e r s   o n l y   o n   M u s t H a v e   ( r o w s   f o r   b o t h   0   A N D   1   e x i s t s )  
 - - A d d e d :   D I S T I N C T  
 - - B y   d e s i g n ;   i t   s e e m s   p o s s i b l e   t o   h a v e   m u l t p l e   p r e f e r e n c e s   p e r   d a y  
 - - E T L - t o o l   c a n ' t   h a n d l e   t h i s !  
  
 S E L E C T   D I S T I N C T  
 	 d a t e _ i d 	 	 	 	 	 	 =   d s d . d a t e _ i d ,    
 	 i n t e r v a l _ i d 	 	 	 	 	 =   d i . i n t e r v a l _ i d ,    
 	 p e r s o n _ i d 	 	 	 	 	 =   d p . p e r s o n _ i d ,    
 	 s c e n a r i o _ i d 	 	 	 	 	 =   d s . s c e n a r i o _ i d ,    
 	 p r e f e r e n c e _ t y p e _ i d 	 	 	 =   C A S E    
 	 	 	 	 	 	 	 	 	 	 - - S h i f t   C a t e g o r y   ( s t a n d a r d )   P r e f e r e n c e  
 	 	 	 	 	 	 	 	 	 	 W H E N   I S N U L L ( f . S t a r t T i m e M i n i m u m , ' ' )   +   I S N U L L ( f . E n d T i m e M i n i m u m , ' ' )   +   I S N U L L ( f . S t a r t T i m e M a x i m u m , ' ' )   +   I S N U L L ( f . E n d T i m e M a x i m u m , ' ' )   +     I S N U L L ( f . W o r k T i m e M i n i m u m , ' ' )   +   I S N U L L ( f . W o r k T i m e M a x i m u m , ' ' )   =   ' '   A N D   f . s h i f t _ c a t e g o r y _ c o d e   I S   N O T   N U L L   T H E N   1  
 	 	 	 	 	 	 	 	 	 	 - - D a y   O f f   P r e f e r e n c e  
 	 	 	 	 	 	 	 	 	 	 W H E N   I S N U L L ( f . S t a r t T i m e M i n i m u m , ' ' )   +   I S N U L L ( f . E n d T i m e M i n i m u m , ' ' )   +   I S N U L L ( f . S t a r t T i m e M a x i m u m , ' ' )   +   I S N U L L ( f . E n d T i m e M a x i m u m , ' ' )   +     I S N U L L ( f . W o r k T i m e M i n i m u m , ' ' )   +   I S N U L L ( f . W o r k T i m e M a x i m u m , ' ' )   =   ' '   A N D   f . d a y _ o f f _ n a m e   I S   N O T   N U L L   T H E N   2  
 	 	 	 	 	 	 	 	 	 	 - - E x t e n d e d   P r e f e r e n c e  
 	 	 	 	 	 	 	 	 	 	 W H E N   f . S t a r t T i m e M i n i m u m   I S   N O T   N U L L   O R   f . E n d T i m e M i n i m u m   I S   N O T   N U L L   O R   f . S t a r t T i m e M a x i m u m   I S   N O T   N U L L   O R   f . E n d T i m e M a x i m u m   I S   N O T   N U L L   O R   f . W o r k T i m e M i n i m u m   I S   N O T   N U L L   O R   f . W o r k T i m e M a x i m u m   I S   N O T   N U L L   T H E N   3  
 	 	 	 	 	 	 	 	     E N D ,  
 	 s h i f t _ c a t e g o r y _ i d 	 	 	 =   i s n u l l ( s c . s h i f t _ c a t e g o r y _ i d , - 1 ) ,    
 	 d a y _ o f f _ i d 	 	 	 	 	 =   i s n u l l ( d d o . d a y _ o f f _ i d , - 1 ) ,  
 	 p r e f e r e n c e s _ r e q u e s t e d _ c o u n t 	 =   1 ,    
 	 p r e f e r e n c e s _ a c c e p t e d _ c o u n t 	 =   f . p r e f e r e n c e _ a c c e p t e d ,   - - k o l l a   h u r   v i   g � r   h � r  
 	 p r e f e r e n c e s _ d e c l i n e d _ c o u n t 	 =   f . p r e f e r e n c e _ d e c l i n e d ,     - - k o l l a   h u r   v i   g � r   h � r  
 	 b u s i n e s s _ u n i t _ i d 	 	 	 =   d p . b u s i n e s s _ u n i t _ i d ,    
 	 d a t a s o u r c e _ i d 	 	 	 	 =   f . d a t a s o u r c e _ i d ,    
 	 d a t a s o u r c e _ u p d a t e _ d a t e 	 	 =   f . d a t a s o u r c e _ u p d a t e _ d a t e  
 F R O M    
 	 (  
 	 	 S E L E C T   *   F R O M   S t a g e . s t g _ s c h e d u l e _ p r e f e r e n c e   W H E R E   c o n v e r t ( s m a l l d a t e t i m e , f l o o r ( c o n v e r t ( d e c i m a l ( 1 8 , 8 ) , r e s t r i c t i o n _ d a t e   ) ) )   b e t w e e n   @ s t a r t _ d a t e   A N D   @ e n d _ d a t e  
 	 )   A S   f  
 J O I N  
 	 m a r t . d i m _ p e r s o n 	 	 d p  
 O N  
 	 f . p e r s o n _ c o d e 	 	 = 	 	 	 d p . p e r s o n _ c o d e 	 A N D  
 	 f . r e s t r i c t i o n _ d a t e 	 	 B E T W E E N 	 	 d p . v a l i d _ f r o m _ d a t e 	 A N D   d p . v a l i d _ t o _ d a t e     - - I s   p e r s o n   v a l i d   i n   t h i s   r a n g e  
 L E F T   J O I N  
 	 m a r t . d i m _ d a t e 	 	 d s d  
 O N  
 	 f . r e s t r i c t i o n _ d a t e 	 =   d s d . d a t e _ d a t e  
 L E F T   J O I N 	 	 	 	 - - k o m m e r   d e n n a   a t t   t a   b o r t   s e n   o c h   b l i   d a t e o n l y   i   a g e n t e n s   t i d s z o n ? ? ?  
 	 m a r t . d i m _ i n t e r v a l 	 d i  
 O N  
 	 f . i n t e r v a l _ i d   =   d i . i n t e r v a l _ i d     - - F i x   B y   D a v i d :   s t a r t _ i n t e r v a l _ i d   = >   i n t e r v a l _ i d  
 L E F T   J O I N  
 	 m a r t . d i m _ s c e n a r i o 	 d s  
 O N  
 	 f . s c e n a r i o _ c o d e   =   d s . s c e n a r i o _ c o d e  
 L E F T   J O I N  
 	 m a r t . d i m _ s h i f t _ c a t e g o r y   s c  
 O N  
 	 f . s h i f t _ c a t e g o r y _ c o d e   =   s c . s h i f t _ c a t e g o r y _ c o d e  
 I N N E R   J O I N    
 	 m a r t . d i m _ b u s i n e s s _ u n i t   b u  
 O N  
 	 d p . b u s i n e s s _ u n i t _ c o d e   =   b u . b u s i n e s s _ u n i t _ c o d e  
 L E F T   J O I N 	 	 	 	 - - v i   k � r   t i l l s   v i d a r e   p �   d a y _ o f f _ n a m e   s o m   " p r i m a r y   k e y "  
 	 m a r t . d i m _ d a y _ o f f   d d o  
 O N  
 	 f . d a y _ o f f _ n a m e   =   d d o . d a y _ o f f _ n a m e  
  
 - - L E F T   J O I N 	 	 	 	 - - b e h � v e r   i n t e   d e n n a   o m   d e   s � t t s   h � r t  
 - - 	 d i m _ p r e f e r e n c e _ t y p e   d p t  
 - - O N  
 - - 	 d p t . p r e f e r e n c e _ t y p e _ n a m e = f . p r e f e r e n c e _ t y p e _ n a m e  
  
 G O  
 