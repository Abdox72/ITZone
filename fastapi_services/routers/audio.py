from fastapi import APIRouter, File, UploadFile, Depends
from transformers import AutoTokenizer, AutoModelForCausalLM, pipeline
from faster_whisper import WhisperModel
import torch
import tempfile
import os
from typing import Dict, Any
from sqlalchemy.orm import Session

from database import get_db
from security import get_current_active_user
import models

router = APIRouter(
    prefix="/audio",
    tags=["audio"],
    responses={404: {"description": "Not found"}},
)

# 🔍 تحديد نوع الجهاز المتاح (GPU أو CPU)
use_cuda = torch.cuda.is_available()
device = "cuda" if use_cuda else "cpu"
torch_dtype = torch.float16 if use_cuda else torch.float32

print(f"✅ Using device: {device}")

# 🎧 تحميل موديل Whisper (صوت → نص)
whisper_model = WhisperModel("base", device=device, compute_type="float16" if use_cuda else "int8")

# 🤖 تحميل موديل Qwen Chat
model_name = "Qwen/Qwen1.5-1.8B-Chat"
tokenizer = AutoTokenizer.from_pretrained(model_name, trust_remote_code=True)
model = AutoModelForCausalLM.from_pretrained(
    model_name,
    device_map="auto" if use_cuda else None,
    torch_dtype=torch_dtype,
    trust_remote_code=True
)

# ✅ إعداد واجهة التوليد
pipe = pipeline("text-generation", model=model, tokenizer=tokenizer, device=0 if use_cuda else -1)

# ✨ توليد الـ prompt
def generate_prompt(transcript: str) -> str:
    return f"""
لديك نص مكتوب لاجتماع فريق عمل (باللغة العربية أو الإنجليزية).

مطلوب:
1. تلخيص النقاط الرئيسية.
2. استخراج المهام والمسؤولين عنها.
3. استخراج المواعيد النهائية (deadlines).
4. تنظيم الإجابة بشكل منظم.

نص الاجتماع:
\"\"\"{transcript}\"\"\"
"""

@router.post("/analyze-meeting/", response_model=Dict[str, Any])
async def analyze_meeting_audio(
    file: UploadFile = File(...),
    db: Session = Depends(get_db),
    current_user: models.User = Depends(get_current_active_user)
):
    # 📥 حفظ الملف مؤقتًا
    with tempfile.NamedTemporaryFile(delete=False, suffix=".mp3") as tmp:
        tmp.write(await file.read())
        audio_path = tmp.name

    # 📝 تحويل الصوت إلى نص
    segments, _ = whisper_model.transcribe(audio_path)
    transcript = " ".join([segment.text for segment in segments])
    os.remove(audio_path)

    # 🧠 تحليل النص باستخدام Qwen
    prompt = generate_prompt(transcript)
    result = pipe(prompt, max_new_tokens=512, do_sample=False)[0]["generated_text"]

    return {
        "transcript": transcript,
        "analysis": result
    }