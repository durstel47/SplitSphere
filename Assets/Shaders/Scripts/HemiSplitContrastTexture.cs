using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
	[ExecuteInEditMode]
	[AddComponentMenu("Image Effects/HemiSplitContrastTexture")]
	[RequireComponent (typeof(Camera))]
	public class HemiSplitContrastTexture : ImageEffectBase
	{
		/**********************************
		public Texture lSideTexture {
			get { return _lSideTexture;  }
			set { _lSideTexture = value; Debug.Log ("Got ls texture: " + _lSideTexture.name);} }
		public Texture rSideTexture {
			get { return _rSideTexture; }
			set { _rSideTexture = value; Debug.Log ("Got rs texture: " + _rSideTexture.name);} }
		*******/

		public Texture maskTexture {
			get { return _maskTexture; }
			set { _maskTexture = value; } }
		public Vector2 lSideTexOffset{
			get { return _lSideTexOffset; }
			set { _lSideTexOffset = value; } }
		public Vector2 lSideTexScale {
			get { return _lSideTexScale; }
			set { _lSideTexScale = value; } }
		public float lSideContrast {
			get { return _lSideContrast; }
			set { _lSideContrast = value; } }
		public Vector2 rSideTexOffset {
			get { return _rSideTexOffset; }
			set { _rSideTexOffset = value; } }
		public Vector2 rSideTexScale {
			get { return _rSideTexScale; }
			set { _rSideTexScale = value; } }
		public float rSideContrast {
			get { return _rSideContrast; }
			set { _rSideContrast = value; Debug.Log ("Got rs contrast: " + _rSideContrast.ToString());} }
		public Vector2 maskTexOffset {
			get { return _maskTexOffset; }
			set { _maskTexOffset = value; } }
		public Vector2 maskTexScale {
			get { return _maskTexScale; }
			set { _maskTexScale = value; } }
		public float contrast {
			get { return _contrast; }
			set { _contrast = value; } }

		public Texture _lSideTexture;
		public Texture _rSideTexture; 
		public Texture _maskTexture;

		private Vector2 _lSideTexOffset;
		public Vector2 _lSideTexScale;
		private float _lSideContrast = 1f;
		private Vector2 _rSideTexOffset;
		private Vector2 _rSideTexScale;
		private float _rSideContrast = 1f;
		private Vector2 _maskTexOffset;
		private Vector2 _maskTexScale;
		private float _contrast = 1f;


		override protected void Start()
		{
			_lSideTexOffset = new Vector2 (1f, 1f);
			_lSideTexScale = new Vector2 (-1f, -1f);
			_rSideTexOffset = new Vector2 (1f, 1f);
			_rSideTexScale = new Vector2 (-1f, -1f);
			_maskTexOffset = new Vector2 (0.25f, 0f);
			_maskTexScale = new Vector2 (0.5f, 1f);
			Debug.Log ("_lSideTexture " + _lSideTexture.name);
			Debug.Log ("_lSideContrast " + _lSideContrast.ToString ());
			base.Start ();
		}


		// Called by camera to apply image effect
		void OnRenderImage (RenderTexture source, RenderTexture destination) {

			material.SetTexture("_MainTex", _lSideTexture);
			material.SetTextureOffset("_MainTex", _lSideTexOffset);
			material.SetTextureScale ("_MainTex", _lSideTexScale);
			material.SetFloat ("_LeftContrast", _lSideContrast);
			material.SetTexture("_RightTex", _rSideTexture);
			material.SetTextureOffset("_RightTex", _rSideTexOffset);
			material.SetTextureScale ("_RightTex", _rSideTexScale);
			material.SetFloat ("_RightContrast", _rSideContrast);
			material.SetTexture("_MaskTex", _maskTexture);
			material.SetTextureOffset("_MaskTex", _maskTexOffset);
			material.SetTextureScale ("_MaskTex", _maskTexScale);
			material.SetFloat ("_Contrast", _contrast);

			Graphics.Blit (source, destination, material);
		}

	}
}
